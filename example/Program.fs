module NinjaFs.Example

open NinjaFs

let varDecl = VarDecl.create {| Name = "builddir"; Value = "build" |}

let deps = Deps.Gcc

let rule =
    Rule.create
        {| Name = "compile"
           Command = "gcc -c -o $out $in" |}

let build =
    Build.create
        {| RuleName = "compile"
           Inputs = (Build.ExplicitInputs [ "main.c" ], Build.ImplicitInputs [])
           Outputs = (Build.ExplicitOutputs [ "main.o" ], Build.ImplicitOutputs []) |}

printfn "%s" <| VarDecl.display varDecl
printfn "%s" <| Deps.display deps
printfn "%s" <| Rule.display rule
printfn "%s" <| Build.display build
