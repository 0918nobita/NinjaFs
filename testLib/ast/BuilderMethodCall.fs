[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.BuilderMethodCall

type BuilderMethodCall =
    | BuilderMethodCall of
        {| Method: string
           Target: IExpr
           Args: list<IExpr> |}

    interface IExpr with
        member __.IsSimpleExpr = true

        member __.Encoder() = failwith "not implemented"

        member this.ReconstructSourceCode() =
            let (BuilderMethodCall payload) = this
            let target = payload.Target.ReconstructSourceCode()

            let target =
                if payload.Target.IsSimpleExpr then
                    target
                else
                    $"({target})"

            let args =
                if List.isEmpty payload.Args then
                    ""
                else
                    payload.Args
                    |> List.map (fun arg ->
                        let isSimpleExpr = arg.IsSimpleExpr
                        let arg = arg.ReconstructSourceCode()

                        if isSimpleExpr then
                            arg
                        else
                            $"(%s{arg})")
                    |> String.concat ", "
                    |> sprintf ", %s"

            $"%s{target} |>> builder.%s{payload.Method}(_r%s{args})"
