module NinjaFs.DocsGen

open FSharp.Formatting.ApiDocs

let docModel =
    ApiDocs.GenerateModel([ ApiDocInput.FromFile("src/bin/Debug/netstandard2.0/NinjaFs.dll") ], "NinjaFs", [])

let searchIndexEntries = ApiDocs.SearchIndexEntriesForModel(docModel)

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

open Giraffe.ViewEngine

let indexPage =
    let namespaces =
        collection.Namespaces
        |> List.map (fun ns ->
            li
                []
                ([ a [ _href (ns.Url(root = "", collectionName = "", qualify = false, extension = ".html")) ] [
                       code [] [ str ns.Name ]
                   ] ]
                 @ (ns.NamespaceDocs
                    |> Option.map (fun doc -> [ span [] [ rawText doc.Summary.HtmlText ] ])
                    |> Option.defaultValue [])))

    html [ _lang "ja" ] [
        head [] [
            meta [ _charset "utf-8" ]
            link [ _rel "stylesheet"
                   _href "https://cdn.jsdelivr.net/npm/firacode@6.2.0/distr/fira_code.css" ]
            link [ _rel "stylesheet"
                   _href "global.css" ]
            title [] [ str "NinjaFs API Reference" ]
        ]
        body [] [
            main
                []
                ([ h2 [] [ str "NinjaFs API Reference" ] ]
                 @ [ ul [] namespaces ])
        ]
    ]

let referencePages =
    collection.Namespaces
    |> List.map (fun ns ->
        let name = ns.Name

        let entities =
            ns.Entities
            |> List.map (fun entity ->
                li [] [
                    code [] [ str entity.Name ]
                    span [] [
                        rawText entity.Comment.Summary.HtmlText
                    ]
                ])

        (ns.Url(root = "public/", collectionName = "", qualify = false, extension = ".html"),
         html [ _lang "ja" ] [
             head [] [
                 meta [ _charset "utf-8" ]
                 link [ _rel "stylesheet"
                        _href "https://cdn.jsdelivr.net/npm/firacode@6.2.0/distr/fira_code.css" ]
                 link [ _rel "stylesheet"
                        _href "../global.css" ]
                 title [] [
                     str $"{name} | NinjaFs API Reference"
                 ]
             ]
             body [] [
                 main [] [
                     h2 [] [ str name ]
                     ul [] entities
                 ]
             ]
         ]))

open System.IO

indexPage
|> RenderView.AsString.htmlDocument
|> (fun content -> File.WriteAllText("public/index.html", content + "\n"))

Directory.CreateDirectory("public/reference")
|> ignore

referencePages
|> List.map (fun (path, htmlDoc) ->
    let content = RenderView.AsString.htmlDocument htmlDoc

    File.WriteAllTextAsync(path, content)
    |> Async.AwaitTask)
|> Async.Parallel
|> Async.RunSynchronously
|> ignore
