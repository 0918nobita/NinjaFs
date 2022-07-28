[<AutoOpen>]
module NinjaFs.Example.Ast.StringLiteral

open Thoth.Json.Net

type StringLiteral =
    | StringLiteral of string

    interface IExpr with
        member this.encoder() =
            let (StringLiteral value) = this

            Encode.object [ ("type", Encode.string "stringLiteral")
                            ("value", Encode.string value) ]
