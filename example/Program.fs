module NinjaFs.Example.Program

open NinjaFs
open NinjaFs.TestLib

let expr =
    <@ ninja {
        var "builddir" "build"

        rule "compile" "gcc -c -o $out $in"

        rule "link" "gcc -o $out $in"

        yield VarDecl(VarDecl.create {| Name = "foo"; Value = "bar" |})

        yield! ninja { var "hoge" "fuga" }
    } @>

open System.IO

File.WriteAllText("ce.json", Expr.toJsonStr expr)

File.WriteAllText("ce.txt", Expr.toSrcStr expr)
