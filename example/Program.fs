module NinjaFs.Example.Program

open NinjaFs
open NinjaFs.TestLib

let expr =
    ninja {
        var "builddir" "build"

        rule "compile" "gcc -c -o $out $in"

        rule "link" "gcc -o $out $in"

        yield VarDecl(VarDecl.create {| Name = "foo"; Value = "bar" |})

        yield!
            Configuration.singleton (
                VarDecl
                <| VarDecl.create {| Name = "hoge"; Value = "fuga" |}
            )
    }

writeSnapshot "test.ast.json" expr

(*
open FSharp.Quotations
open FSharp.Quotations.Patterns

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
*)
