[<AutoOpen>]
module NinjaFs.TestLib.Ast.Expr

type IExpr =
    abstract member encoder: unit -> Thoth.Json.Net.JsonValue
