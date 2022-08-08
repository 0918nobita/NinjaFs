module NinjaFs.DocsGen.Search

open FSharp.Formatting.ApiDocs
open Thoth.Json.Net

type ApiDocsSearchIndexEntry with
    member this.ToJson() : JsonValue =
        Encode.object [ ("title", Encode.string this.title)
                        ("content", Encode.string this.content)
                        ("uri", Encode.string this.uri) ]

let genSearchIndex docModel =
    docModel
    |> ApiDocs.SearchIndexEntriesForModel
    |> Array.map (fun entry -> entry.ToJson())
    |> Encode.array
    |> Encode.toString 2
    |> (fun content -> System.IO.File.WriteAllText("public/search_index.json", content + "\n"))
