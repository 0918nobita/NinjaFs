module NinjaFs.Example

(*
Fantomas.Core.CodeFormatter.FormatDocumentAsync(false, "module Program\nlet a = 2+2")
|> Async.RunSynchronously
|> printfn "[result]\n%s"
*)

open FSharp.Quotations
open FSharp.Quotations.Patterns
open NinjaFs

let expr =
    ninja {
        var "builddir" "build"

        rule "compile" "gcc -c -o $out $in"

        rule "link" "gcc -o $out $in"

        yield VarDecl(VarDecl.create {| Name = "foo"; Value = "bar" |})

    (*
        yield!
            if false then
                ninja { build [ "build/main.o" ] "compile" [ "main.c" ] }
            else
                ninja { build [ "build/main.o" ] "compile" ([ "main.c" ].implicitInput [ "lib.h" ]) }
        *)
    }

let printExpr (expr: Expr<_>) =
    match expr with
    | Call (object, method, args) ->
        printfn "call"
        printfn "    object: %A" object
        printfn "    method: %A" method
        printfn "    args: %A" args
    | _ -> failwith $"Unknown expression: %O{expr}"

printExpr expr

// printfn "config |> Ninja.generate ()"
// config |> Ninja.generate ()
