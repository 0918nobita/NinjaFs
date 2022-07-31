module NinjaFs.Example.Program

open NinjaFs
open NinjaFs.TestLib

let expr =
    <@ ninja {
        var "builddir" "build"

        rule "compile" "gcc -c -o $out $in"

        rule "link" "gcc -o $out $in"

        yield VarDecl(VarDecl.create {| Name = "foo"; Value = "bar" |})

        yield!
            Configuration.singleton (
                VarDecl
                <| VarDecl.create {| Name = "hoge"; Value = "fuga" |}
            )
    } @>

writeJson "ce.json" expr

writeSrc "ce.txt" expr
