module NinjaFs.Example

open NinjaFs

let varDecl = VarDecl.create {| Name = "builddir"; Value = "build" |}

let deps = Deps.Gcc

printfn "%s" <| VarDecl.display varDecl
printfn "%s" <| Deps.display deps
