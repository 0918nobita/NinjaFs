[<AutoOpen>]
module NinjaFs.CE

open System.IO

type ConfigurationItem =
    private
    | VarDecl of VarDecl.T
    | Rule of Rule.T
    | Build of Build.T

module ConfigurationItem =
    let display configurationItem =
        match configurationItem with
        | VarDecl varDecl -> VarDecl.display varDecl
        | Rule rule -> Rule.display rule
        | Build build -> Build.display build

type Configuration =
    private
    | Configuration of list<ConfigurationItem>

    static member empty = Configuration []

    member this.addItem(configurationItem: ConfigurationItem) : Configuration =
        let (Configuration items) = this
        Configuration <| items @ [ configurationItem ]

    member this.generate(?filename: string) =
        let filename = filename |> Option.defaultValue "build.ninja"
        let (Configuration items) = this

        let content =
            items
            |> List.map (ConfigurationItem.display)
            |> String.concat "\n"

        File.WriteAllText(filename, content)

type Builder() =
    member __.Yield(_) = Configuration.empty

    [<CustomOperation("var")>]
    member __.Var(config: Configuration, name, value) =
        config.addItem (
            VarDecl
            <| VarDecl.create {| Name = name; Value = value |}
        )

    [<CustomOperation("rule")>]
    member __.Rule(config: Configuration, name: string, command: string) =
        config.addItem (
            Rule
            <| Rule.create {| Name = name; Command = command |}
        )

let ninja = Builder()
