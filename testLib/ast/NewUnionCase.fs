[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.NewUnionCase

open Thoth.Json.Net

type NewUnionCase =
    | NewUnionCase of {| Name: string; Fields: list<IExpr> |}

    interface IExpr with
        member this.encoder() =
            let (NewUnionCase payload) = this

            Encode.object [ ("type", Encode.string "newUnionCase")
                            ("name", Encode.string payload.Name)
                            ("fields",
                             payload.Fields
                             |> List.map (fun field -> field.encoder ())
                             |> Encode.list) ]
