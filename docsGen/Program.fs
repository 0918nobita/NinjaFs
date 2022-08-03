module NinjaFs.DocsGen

open FSharp.Formatting.ApiDocs

let docModel =
    ApiDocs.GenerateModel([ ApiDocInput.FromFile("src/bin/Debug/netstandard2.0/NinjaFs.dll") ], "NinjaFs", [])

docModel.Collection.Namespaces
|> List.map (fun ns -> ns.Name)
|> printfn "namespaces: %A"
