[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.BuilderKeyword

open Thoth.Json.Net

type BuilderKeyword =
    | BuilderKeyword

    interface IExpr with
        member __.encoder() =
            Encode.object [ ("type", Encode.string "builderKeyword") ]
