[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.Lambda

open Thoth.Json.Net

type Lambda =
    | Lambda of {| VarName: string; Body: IExpr |}

    interface IExpr with
        member __.IsSimpleExpr = false

        member this.Encoder() =
            let (Lambda payload) = this

            Encode.object [ ("type", Encode.string "lambda")
                            ("varName", Encode.string payload.VarName)
                            ("body", payload.Body.Encoder()) ]

        member this.ReconstructSourceCode() =
            let (Lambda payload) = this
            let body = payload.Body.ReconstructSourceCode()
            $"fun %s{payload.VarName} -> %s{body}"
