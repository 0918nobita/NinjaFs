[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.VarRef

open Thoth.Json.Net

type VarRef =
    | VarRef of string

    interface IExpr with
        member this.encoder() =
            let (VarRef name) = this

            Encode.object [ ("type", Encode.string "varRef")
                            ("name", Encode.string name) ]
