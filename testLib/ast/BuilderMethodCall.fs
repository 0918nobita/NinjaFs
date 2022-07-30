[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.BuilderMethodCall

open Thoth.Json.Net

type BuilderMethodCall =
    | BuilderMethodCall of
        {| Target: IExpr
           Chain: list<{| Method: string; Args: list<IExpr> |}> |}

    interface IExpr with
        member __.IsSimpleExpr = true

        member this.Encoder() =
            let (BuilderMethodCall payload) = this

            Encode.object [ ("type", Encode.string "builderMethodCall")
                            ("target", payload.Target.Encoder())
                            ("chain",
                             payload.Chain
                             |> List.map (fun call ->
                                 Encode.object [ ("method", Encode.string call.Method)
                                                 ("args",
                                                  call.Args
                                                  |> List.map (fun arg -> arg.Encoder())
                                                  |> Encode.list) ])
                             |> Encode.list) ]

        member this.ReconstructSourceCode() =
            let (BuilderMethodCall payload) = this
            let target = payload.Target.ReconstructSourceCode()

            let target =
                if payload.Target.IsSimpleExpr then
                    target
                else
                    $"({target})"

            let chain =
                payload.Chain
                |> List.map (fun call ->
                    let args =
                        if List.isEmpty call.Args then
                            ""
                        else
                            call.Args
                            |> List.map (fun arg ->
                                let isSimpleExpr = arg.IsSimpleExpr
                                let arg = arg.ReconstructSourceCode()

                                if isSimpleExpr then
                                    arg
                                else
                                    $"(%s{arg})")
                            |> String.concat ", "
                            |> sprintf ", %s"

                    $"|>> builder.%s{call.Method}(_r%s{args})")
                |> String.concat " "

            $"%s{target} %s{chain}"
