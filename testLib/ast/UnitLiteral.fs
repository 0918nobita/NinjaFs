[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.UnitLiteral

open Thoth.Json.Net

type UnitLiteral =
    | UnitLiteral

    interface IExpr with
        member __.IsSimpleExpr = true

        member __.Encoder() =
            Encode.object [ ("type", Encode.string "unitLiteral") ]

        member __.ReconstructSourceCode() = "()"
