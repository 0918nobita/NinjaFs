[<AutoOpen>]
module NinjaFs.TestLib.Library

open FSharp.Quotations.Patterns

[<RequireQualifiedAccess>]
type Expr =
    | Application of func: Expr * arg: Expr
    | BuilderMethodCallChain of object: Expr * chain: list<{| Method: string; Args: list<Expr> |}>
    | FunctionCall of func: string * args: list<Expr>
    | Lambda of var: string * body: Expr
    | MethodCall of object: Expr * method: string * args: list<Expr>
    | NewAnonRecord of Map<string, Expr>
    | NewUnionCase of name: string * fields: list<Expr>
    | StaticPropertyGet of object: string * property: string
    | StringLiteral of value: string
    | UnitLiteral
    | VarRef of name: string

open Thoth.Json.Net

module Expr =
    let isSimpleExpr =
        function
        | Expr.NewAnonRecord _
        | Expr.StringLiteral _
        | Expr.UnitLiteral
        | Expr.VarRef _ -> true
        | Expr.Application _
        | Expr.BuilderMethodCallChain _
        | Expr.FunctionCall _
        | Expr.Lambda _
        | Expr.MethodCall _
        | Expr.NewUnionCase _
        | Expr.StaticPropertyGet _ -> false

    let rec jsonEncode =
        function
        | Expr.Application (func, arg) ->
            Encode.object [ ("type", Encode.string "application")
                            ("func", jsonEncode func)
                            ("arg", jsonEncode arg) ]
        | Expr.BuilderMethodCallChain (object, chain) ->
            let chain =
                chain
                |> List.map (fun call ->
                    Encode.object [ ("method", Encode.string call.Method)
                                    ("args", encodeList call.Args) ])
                |> Encode.list

            Encode.object [ ("type", Encode.string "builderMethodCallChain")
                            ("object", jsonEncode object)
                            ("chain", chain) ]
        | Expr.FunctionCall (func, args) ->
            Encode.object [ ("type", Encode.string "functionCall")
                            ("func", Encode.string func)
                            ("args", encodeList args) ]
        | Expr.Lambda (var, body) ->
            Encode.object [ ("type", Encode.string "lambda")
                            ("var", Encode.string var)
                            ("body", jsonEncode body) ]
        | Expr.MethodCall (object, method, args) ->
            Encode.object [ ("type", Encode.string "methodCall")
                            ("object", jsonEncode object)
                            ("method", Encode.string method)
                            ("args", encodeList args) ]
        | Expr.NewAnonRecord fields ->
            fields
            |> Map.map (fun _k -> jsonEncode)
            |> Map.toList
            |> Encode.object
        | Expr.NewUnionCase (name, fields) ->
            Encode.object [ ("type", Encode.string "newUnionCase")
                            ("name", Encode.string name)
                            ("fields", encodeList fields) ]
        | Expr.StaticPropertyGet (object, property) ->
            Encode.object [ ("type", Encode.string "staticPropertyGet")
                            ("object", Encode.string object)
                            ("property", Encode.string property) ]
        | Expr.StringLiteral value ->
            Encode.object [ ("type", Encode.string "stringLiteral")
                            ("value", Encode.string value) ]
        | Expr.UnitLiteral -> Encode.object [ ("type", Encode.string "unitLiteral") ]
        | Expr.VarRef name ->
            Encode.object [ ("type", Encode.string "varRef")
                            ("name", Encode.string name) ]

    and private encodeList list =
        list |> List.map jsonEncode |> Encode.list

    let rec toSrc: Expr -> string =
        function
        | Expr.Application (func, arg) ->
            let func = toSrcWithParen func
            let arg = toSrcWithParen arg
            $"%s{func} %s{arg}"
        | Expr.BuilderMethodCallChain (object, chain) ->
            let object = toSrcWithParen object

            let chain =
                chain
                |> List.map (fun call ->
                    let args =
                        call.Args
                        |> List.map toSrcWithParen
                        |> String.concat ", "

                    $"|>> builder_at.%s{call.Method}(%s{args})")
                |> String.concat " "

            $"%s{object} %s{chain}"
        | Expr.FunctionCall (func, args) ->
            let args =
                args
                |> List.map toSrcWithParen
                |> String.concat " "

            $"%s{func} %s{args}"
        | Expr.Lambda (var, body) ->
            let var = var.Replace("@", "_at")
            let body = toSrc body
            $"fun %s{var} -> %s{body}"
        | Expr.MethodCall (object, method, args) ->
            let object = toSrcWithParen object

            let args =
                args
                |> List.map toSrcWithParen
                |> String.concat ", "

            $"%s{object}.%s{method}(%s{args})"
        | Expr.NewAnonRecord fields ->
            let fields =
                fields
                |> Map.map (fun k v ->
                    let v = toSrcWithParen v
                    $"%s{k} = %s{v}")
                |> Map.values
                |> String.concat "; "

            $"{{| %s{fields} |}}"
        | Expr.NewUnionCase (name, fields) ->
            let fields =
                if List.isEmpty fields then
                    ""
                else
                    fields
                    |> List.map toSrcWithParen
                    |> String.concat ", "
                    |> sprintf "(%s)"

            $"%s{name}%s{fields}"
        | Expr.StaticPropertyGet (object, property) -> $"%s{object}.%s{property}"
        | Expr.StringLiteral value -> $"\"%s{value}\""
        | Expr.UnitLiteral -> "()"
        | Expr.VarRef name ->
            let name = name.Replace("@", "_at")
            $"%s{name}"

    and private toSrcWithParen (expr: Expr) =
        let src = toSrc expr

        if isSimpleExpr expr then
            src
        else
            $"(%s{src})"

    let private (|NewAnonRecord|_|) (expr: Quotations.Expr) : option<Map<string, Quotations.Expr>> =
        match expr with
        | NewRecord (ty, fields) when ty.Name.StartsWith "<>f__AnonymousType" ->
            ty.GetProperties()
            |> Array.indexed
            |> Array.map (fun (i, property) -> (property.Name, fields[i]))
            |> Map.ofArray
            |> Some
        | _ -> None

    let rec fromQuotationsExpr (expr: Quotations.Expr) : Expr =
        match expr with
        | Application (func, arg) -> Expr.Application(func = fromQuotationsExpr func, arg = fromQuotationsExpr arg)
        | Call (object, method, args) ->
            match object with
            | Some object ->
                let object = fromQuotationsExpr object
                let args = args |> List.map fromQuotationsExpr
                Expr.MethodCall(object, method.Name, args)
            | None ->
                let args = args |> List.map fromQuotationsExpr

                let func =
                    $"%s{method.DeclaringType.Namespace}.%s{method.DeclaringType.Name}.%s{method.Name}"

                Expr.FunctionCall(func, args)
        | Lambda (var, body) ->
            let body = fromQuotationsExpr body
            Expr.Lambda(var.Name, body)
        | NewAnonRecord fields ->
            let fields = fields |> Map.map (fun _ -> fromQuotationsExpr)

            Expr.NewAnonRecord fields
        | NewUnionCase (unionCaseInfo, fields) ->
            let fields = fields |> List.map fromQuotationsExpr

            Expr.NewUnionCase(unionCaseInfo.Name, fields)
        | PropertyGet (None, prop, []) -> Expr.StaticPropertyGet(object = prop.DeclaringType.Name, property = prop.Name)
        | Var var -> Expr.VarRef var.Name
        | Value (:? unit, ty) when ty = typeof<unit> -> Expr.UnitLiteral
        | Value (:? string as v, ty) when ty = typeof<string> -> Expr.StringLiteral v
        | ValueWithName (_value, _ty, name) -> Expr.VarRef name
        | _ -> failwith $"Unsupported expression: %A{expr}"

    let rec flatten (expr: Expr) : Expr =
        match expr with
        | Expr.BuilderMethodCallChain _
        | Expr.StaticPropertyGet _
        | Expr.StringLiteral _
        | Expr.UnitLiteral
        | Expr.VarRef _ -> expr
        | Expr.Application (func, arg) -> Expr.Application(func = flatten func, arg = flatten arg)
        | Expr.FunctionCall (func, args) -> Expr.FunctionCall(func, args |> List.map flatten)
        | Expr.Lambda (var, body) -> Expr.Lambda(var, flatten body)
        | Expr.NewAnonRecord fields ->
            fields
            |> Map.map (fun _ -> flatten)
            |> Expr.NewAnonRecord
        | Expr.NewUnionCase (name, fields) -> Expr.NewUnionCase(name, fields |> List.map flatten)
        | Expr.MethodCall (Expr.VarRef "builder@", method, arg :: args) ->
            match flatten arg with
            | Expr.BuilderMethodCallChain (object, chain) ->
                Expr.BuilderMethodCallChain(object, chain @ [ {| Method = method; Args = args |} ])
            | arg ->
                Expr.BuilderMethodCallChain(
                    arg,
                    [ {| Method = method
                         Args = args |> List.map flatten |} ]
                )
        | Expr.MethodCall (object, method, args) ->
            let object = flatten object
            let args = args |> List.map flatten
            Expr.MethodCall(object, method, args)

    let toJsonStr (expr: Quotations.Expr) =
        let content =
            expr
            |> fromQuotationsExpr
            |> flatten
            |> jsonEncode
            |> Encode.toString 2

        $"{content}\n"

    let toSrcStr (expr: Quotations.Expr) =
        let src = expr |> fromQuotationsExpr |> flatten |> toSrc

        Fantomas.Core.CodeFormatter.FormatDocumentAsync(false, src)
        |> Async.RunSynchronously
