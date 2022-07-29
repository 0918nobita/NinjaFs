[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.BuilderKeyword

open Thoth.Json.Net

type BuilderKeyword =
    | BuilderKeyword

    interface IExpr with
        member __.IsSimpleExpr = true

        member __.Encoder() =
            Encode.object [ ("type", Encode.string "builderKeyword") ]

        member __.ReconstructSourceCode() = "builder"
