[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.VarRef

open Thoth.Json.Net

type VarRef =
    | VarRef of string

    interface IExpr with
        member __.IsSimpleExpr = true

        member this.Encoder() =
            let (VarRef name) = this

            Encode.object [ ("type", Encode.string "varRef")
                            ("name", Encode.string name) ]

        member this.ReconstructSourceCode() =
            let (VarRef name) = this
            name
