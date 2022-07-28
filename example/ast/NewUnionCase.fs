[<AutoOpen>]
module NinjaFs.Example.Ast.NewUnionCase

open Thoth.Json.Net

type NewUnionCase =
    | NewUnionCase of
        {| Name: string
           Fields: Map<string, IExpr> |}

    interface IExpr with
        member this.encoder() =
            let (NewUnionCase payload) = this

            Encode.object [ ("type", Encode.string "newUnionCase")
                            ("name", Encode.string payload.Name)
                            ("fields",
                             payload.Fields
                             |> Map.map (fun _ v -> v.encoder ())
                             |> Map.toList
                             |> Encode.object) ]
