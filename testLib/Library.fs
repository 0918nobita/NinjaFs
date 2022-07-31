[<AutoOpen>]
module NinjaFs.TestLib.Library

open FSharp.Quotations.Patterns

[<RequireQualifiedAccess>]
type Expr =
    | FunctionCall of func: string * args: list<Expr>
    | Lambda of var: string * body: Expr
    | MethodCall of object: Expr * method: string * args: list<Expr>
    | NewAnonRecord of Map<string, Expr>
    | NewUnionCase of name: string * fields: list<Expr>
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
        | Expr.FunctionCall _
        | Expr.Lambda _
        | Expr.MethodCall _
        | Expr.NewUnionCase _ -> false

    let rec jsonEncode =
        function
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
            |> Map.map (fun _k v -> jsonEncode v)
            |> Map.toList
            |> Encode.object
        | Expr.NewUnionCase (name, fields) ->
            Encode.object [ ("type", Encode.string "newUnionCase")
                            ("name", Encode.string name)
                            ("fields", encodeList fields) ]
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
        | Expr.FunctionCall (func, args) ->
            let args =
                args
                |> List.map toSrcWithParen
                |> String.concat " "

            $"%s{func} %s{args}"
        | Expr.Lambda (var, body) ->
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
                |> Map.toList
                |> List.map snd
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
        | Expr.StringLiteral value -> $"\"%s{value}\""
        | Expr.UnitLiteral -> "()"
        | Expr.VarRef name -> $"%s{name}"

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
            let fields =
                fields
                |> Map.map (fun _ field -> fromQuotationsExpr field)

            Expr.NewAnonRecord fields
        | NewUnionCase (unionCaseInfo, fields) ->
            let fields = fields |> List.map fromQuotationsExpr

            Expr.NewUnionCase(unionCaseInfo.Name, fields)
        | Var var -> Expr.VarRef var.Name
        | Value (:? unit, ty) when ty = typeof<unit> -> Expr.UnitLiteral
        | Value (:? string as v, ty) when ty = typeof<string> -> Expr.StringLiteral v
        | ValueWithName (_value, _ty, name) -> Expr.VarRef name
        | _ -> failwith $"Unsupported expression: %A{expr}"

let writeSnapshot (filename: string) (expr: Quotations.Expr) =
    let content =
        expr
        |> Expr.fromQuotationsExpr
        |> Expr.jsonEncode
        |> Encode.toString 2

    System.IO.File.WriteAllText(filename, $"{content}\n")
