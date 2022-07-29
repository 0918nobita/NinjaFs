[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.FunctionCall

open Thoth.Json.Net

type FunctionCall =
    | FunctionCall of {| Func: string; Args: list<IExpr> |}

    interface IExpr with
        member __.IsSimpleExpr = false

        member this.Encoder() =
            let (FunctionCall payload) = this

            Encode.object [ ("type", Encode.string "functionCall")
                            ("func", Encode.string payload.Func)
                            ("args",
                             payload.Args
                             |> List.map (fun arg -> arg.Encoder())
                             |> Encode.list) ]

        member this.ReconstructSourceCode() =
            let (FunctionCall payload) = this

            let args =
                payload.Args
                |> List.map (fun arg ->
                    let isSimpleExpr = arg.IsSimpleExpr
                    let arg = arg.ReconstructSourceCode()

                    if isSimpleExpr then
                        arg
                    else
                        $"(%s{arg})")
                |> String.concat " "

            $"%s{payload.Func} %s{args}"
