[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.NewUnionCase

open Thoth.Json.Net

type NewUnionCase =
    | NewUnionCase of {| Name: string; Fields: list<IExpr> |}

    interface IExpr with
        member __.IsSimpleExpr = false

        member this.Encoder() =
            let (NewUnionCase payload) = this

            Encode.object [ ("type", Encode.string "newUnionCase")
                            ("name", Encode.string payload.Name)
                            ("fields",
                             payload.Fields
                             |> List.map (fun field -> field.Encoder())
                             |> Encode.list) ]

        member this.ReconstructSourceCode() =
            let (NewUnionCase payload) = this

            let fields =
                if not <| List.isEmpty payload.Fields then
                    payload.Fields
                    |> List.map (fun field ->
                        let isSimpleExpr = field.IsSimpleExpr
                        let field = field.ReconstructSourceCode()

                        if isSimpleExpr then
                            field
                        else
                            $"({field})")
                    |> String.concat ", "
                    |> sprintf "(%s)"
                else
                    ""

            $"%s{payload.Name}%s{fields}"
