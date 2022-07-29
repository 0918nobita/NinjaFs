[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.StringLiteral

open Thoth.Json.Net

type StringLiteral =
    | StringLiteral of string

    interface IExpr with
        member __.IsSimpleExpr = true

        member this.Encoder() =
            let (StringLiteral value) = this

            Encode.object [ ("type", Encode.string "stringLiteral")
                            ("value", Encode.string value) ]

        member this.ReconstructSourceCode() =
            let (StringLiteral value) = this
            $"\"%s{value}\""
