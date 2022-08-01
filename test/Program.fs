open Expecto
open NinjaFs
open NinjaFs.TestLib
open VerifyExpecto

[<Tests>]
let varTest =
    testTask "var" {
        let expr = <@ ninja { var "builddir" "build" } @>

        do! Verifier.Verify("var", Expr.toSrcStr expr)
    }

[<Tests>]
let ruleTest =
    testTask "rule" {
        let expr = <@ ninja { rule "compile" "gcc -c -o $out $in" } @>

        do! Verifier.Verify("rule", Expr.toSrcStr expr)
    }

[<EntryPoint>]
let main args = runTestsInAssembly defaultConfig args
