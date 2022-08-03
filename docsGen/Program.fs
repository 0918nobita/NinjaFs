module NinjaFs.DocsGen

open FSharp.Formatting.ApiDocs

let docModel =
    ApiDocs.GenerateModel([ ApiDocInput.FromFile("src/bin/Debug/netstandard2.0/NinjaFs.dll") ], "NinjaFs", [])

let collection = docModel.Collection

let rec dumpEntities (indent: int) : list<ApiDocEntity> -> unit =
    function
    | [] -> ()
    | entities ->
        let indentStr = String.replicate indent " "

        entities
        |> List.iter (fun entity ->
            printfn "%s[%s]" indentStr entity.Name

            entity.AllMembers
            |> List.iter (fun mem -> printfn "%s  - %s" indentStr mem.Name)

            dumpEntities (indent + 2) entity.NestedEntities)

collection.Namespaces
|> List.iter (fun ns ->
    printfn "%s" ns.Name
    ns.Entities |> dumpEntities 0)
