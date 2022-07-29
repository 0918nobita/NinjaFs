module NinjaFs.Example.Program

open NinjaFs

let expr =
    ninja {
        var "builddir" "build"

        rule "compile" "gcc -c -o $out $in"

        rule "link" "gcc -o $out $in"

        yield VarDecl(VarDecl.create {| Name = "foo"; Value = "bar" |})
    }

open FSharp.Quotations
open FSharp.Quotations.Patterns

let (|NewAnonRecord|_|) (expr: Expr) : option<Map<string, Expr>> =
    match expr with
    | NewRecord (ty, fields) when ty.Name.StartsWith "<>f__AnonymousType" ->
        ty.GetProperties()
        |> Array.indexed
        |> Array.map (fun (i, property) -> (property.Name, fields[i]))
        |> Map.ofArray
        |> Some
    | _ -> None

type Expr with
    static member ToIExpr(target: Expr) : Ast.Expr.IExpr =
        match target with
        | Call (object, method, args) ->
            match object with
            | Some object ->
                let object = Expr.ToIExpr object
                let args = args |> List.map Expr.ToIExpr

                methodCall
                    {| Object = object
                       Method = method.Name
                       Args = args |}
            | None ->
                let args = args |> List.map Expr.ToIExpr
                functionCall {| Func = method.Name; Args = args |}
        | Lambda (var, body) ->
            let body = Expr.ToIExpr body
            lambda {| VarName = var.Name; Body = body |}
        | NewAnonRecord fields ->
            let fields =
                fields
                |> Map.map (fun _ field -> Expr.ToIExpr field)

            newAnonRecord fields
        | NewUnionCase (unionCaseInfo, fields) ->
            let fields = fields |> List.map Expr.ToIExpr

            newUnionCase
                {| Name = unionCaseInfo.Name
                   Fields = fields |}
        | Value (obj, ty) when obj = () && ty = typeof<unit> -> unitLiteral
        | Value (obj, ty) when ty = typeof<string> -> stringLiteral (obj :?> string)
        | ValueWithName (_value, _ty, "builder@") -> builderKeyword
        | _ -> failwith $"Unsupported expression: %A{target}"

open Thoth.Json.Net
printfn "%A" (Encode.toString 2 ((Expr.ToIExpr expr).encoder ()))

let rec printExpr (expr: Expr) : string =
    match expr with
    | Call (object, method, args) ->
        match object with
        | Some object ->
            let object = printExpr object

            let args =
                args
                |> List.map (printExpr >> sprintf "(%s)")
                |> String.concat ", "

            $"%s{object}.%s{method.Name}(%s{args})"
        | None ->
            let args =
                args
                |> List.map (printExpr >> sprintf "(%s)")
                |> String.concat " "

            $"%s{method.Name} %s{args}"
    | ValueWithName (_value, _ty, "builder@") -> "builder"
    | Lambda (var, body) ->
        let body = printExpr body
        $"fun {var.Name} -> {body}"
    | Value (obj, ty) when obj = () && ty = typeof<unit> -> "()"
    | Value (obj, ty) when ty = typeof<string> ->
        let str = obj :?> string
        $"\"{str}\""
    | NewUnionCase (unionCaseInfo, fields) ->
        let fields =
            fields
            |> List.map (printExpr >> sprintf "(%s)")
            |> String.concat ", "

        $"%s{unionCaseInfo.Name}(%s{fields})"
    | NewRecord (ty, fields) ->
        let fields =
            ty.GetProperties()
            |> Array.indexed
            |> Array.map (fun (i, property) ->
                let value = printExpr fields[i]
                $"%s{property.Name} = (%s{value})")
            |> String.concat "; "

        $"{{| %s{fields} |}}"
    | _ -> failwith $"Unknown expression: %O{expr}"

let printedExpr = printExpr expr

Fantomas.Core.CodeFormatter.FormatDocumentAsync(false, printedExpr)
|> Async.RunSynchronously
|> printfn "%s"
