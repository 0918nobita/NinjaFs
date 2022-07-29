[<AutoOpen>]
module NinjaFs.TestLib.Ast.MethodCall

open Thoth.Json.Net

type MethodCall =
    | MethodCall of
        {| Object: IExpr
           Method: string
           Args: list<IExpr> |}

    interface IExpr with
        member this.encoder() =
            let (MethodCall payload) = this

            Encode.object [ ("type", Encode.string "methodCall")
                            ("object", payload.Object.encoder ())
                            ("method", Encode.string payload.Method)
                            ("args",
                             payload.Args
                             |> List.map (fun arg -> arg.encoder ())
                             |> Encode.list) ]
