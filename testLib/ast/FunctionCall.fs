[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.FunctionCall

open Thoth.Json.Net

type FunctionCall =
    | FunctionCall of {| Func: string; Args: list<IExpr> |}

    interface IExpr with
        member this.encoder() =
            let (FunctionCall payload) = this

            Encode.object [ ("type", Encode.string "functionCall")
                            ("func", Encode.string payload.Func)
                            ("args",
                             payload.Args
                             |> List.map (fun arg -> arg.encoder ())
                             |> Encode.list) ]
