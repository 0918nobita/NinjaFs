[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.Lambda

open Thoth.Json.Net

type Lambda =
    | Lambda of {| VarName: string; Body: IExpr |}

    interface IExpr with
        member this.encoder() =
            let (Lambda payload) = this

            Encode.object [ ("type", Encode.string "lambda")
                            ("varName", Encode.string payload.VarName)
                            ("body", payload.Body.encoder ()) ]
