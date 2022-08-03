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

open Giraffe.ViewEngine

let htmlDoc =
    let namespaces =
        collection.Namespaces
        |> List.map (fun ns ->
            li [] [
                a [ _href (ns.Url(root = "", collectionName = "NinjaFs", qualify = true, extension = ".html")) ] [
                    str ns.Name
                ]
            ])

    html [ _lang "ja" ] [
        head [] [
            meta [ _charset "utf-8" ]
            title [] [ str "NinjaFs API Reference" ]
        ]
        body
            []
            ([ h2 [] [ str "NinjaFs API Reference" ] ]
             @ [ ul [] namespaces ])
    ]

htmlDoc
|> RenderView.AsString.htmlDocument
|> (fun content -> System.IO.File.WriteAllText("docs/index.html", content + "\n"))
