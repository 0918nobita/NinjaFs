[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.NewAnonRecord

open Thoth.Json.Net

type NewAnonRecord =
    | NewAnonRecord of Map<string, IExpr>

    interface IExpr with
        member this.encoder() =
            let (NewAnonRecord payload) = this

            payload
            |> Map.map (fun _ v -> v.encoder ())
            |> Map.toList
            |> Encode.object
