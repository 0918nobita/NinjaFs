[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.UnitLiteral

open Thoth.Json.Net

type UnitLiteral =
    | UnitLiteral

    interface IExpr with
        member __.encoder() =
            Encode.object [ ("type", Encode.string "unitLiteral") ]
