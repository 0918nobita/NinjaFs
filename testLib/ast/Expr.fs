[<AutoOpen>]
module NinjaFs.TestLib.Ast.Expr

type IExpr =
    abstract member IsSimpleExpr: bool
    abstract member Encoder: unit -> Thoth.Json.Net.JsonValue
    abstract member ReconstructSourceCode: unit -> string
