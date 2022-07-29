[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.MethodCall

open Thoth.Json.Net

type MethodCall =
    | MethodCall of
        {| Object: IExpr
           Method: string
           Args: list<IExpr> |}

    interface IExpr with
        member __.IsSimpleExpr = false

        member this.Encoder() =
            let (MethodCall payload) = this

            Encode.object [ ("type", Encode.string "methodCall")
                            ("object", payload.Object.Encoder())
                            ("method", Encode.string payload.Method)
                            ("args",
                             payload.Args
                             |> List.map (fun arg -> arg.Encoder())
                             |> Encode.list) ]

        member this.ReconstructSourceCode() =
            let (MethodCall payload) = this
            let object = payload.Object.ReconstructSourceCode()

            let object =
                if payload.Object.IsSimpleExpr then
                    object
                else
                    $"({object})"

            let args =
                payload.Args
                |> List.map (fun arg ->
                    let isSimpleExpr = arg.IsSimpleExpr
                    let arg = arg.ReconstructSourceCode()
                    if isSimpleExpr then arg else $"({arg})")
                |> String.concat ", "

            $"%s{object}.%s{payload.Method}(%s{args})"
