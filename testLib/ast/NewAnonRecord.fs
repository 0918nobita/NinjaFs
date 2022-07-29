[<AutoOpen>]
module internal NinjaFs.TestLib.Ast.NewAnonRecord

open Thoth.Json.Net

type NewAnonRecord =
    | NewAnonRecord of Map<string, IExpr>

    interface IExpr with
        member __.IsSimpleExpr = true

        member this.Encoder() =
            let (NewAnonRecord payload) = this

            payload
            |> Map.map (fun _ v -> v.Encoder())
            |> Map.toList
            |> Encode.object

        member this.ReconstructSourceCode() =
            let (NewAnonRecord fields) = this

            let fields =
                fields
                |> Map.map (fun k v ->
                    let isSimpleExpr = v.IsSimpleExpr
                    let v = v.ReconstructSourceCode()

                    if isSimpleExpr then
                        $"{k} = {v}"
                    else
                        $"{k} = (%s{v})")
                |> Map.toList
                |> List.map snd
                |> String.concat "; "

            $"{{| %s{fields} |}}"
