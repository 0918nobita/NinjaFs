module NinjaFs.DocsGen

open FSharp.Formatting.ApiDocs

let docModel =
    ApiDocs.GenerateModel([ ApiDocInput.FromFile("src/bin/Debug/netstandard2.0/NinjaFs.dll") ], "NinjaFs", [])

let collection = docModel.Collection

collection.Namespaces
|> List.iter (fun ns ->
    printfn "%s" ns.Name

    ns.Entities
    |> List.iter (fun entity ->
        printfn "  %s" entity.Name

        printfn "    (%s)"
        <| entity.Url("https://0918nobita.github.io/NinjaFs/", collection.CollectionName, true, ".fs")))
