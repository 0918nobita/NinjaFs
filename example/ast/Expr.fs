[<AutoOpen>]
module NinjaFs.Example.Ast.Expr

type IExpr =
    abstract member encoder: unit -> Thoth.Json.Net.JsonValue
