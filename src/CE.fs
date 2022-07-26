[<AutoOpen>]
module NinjaFs.CE

open System.IO

type ConfigurationItem =
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

    static member singleton item = Configuration [ item ]

    member this.addItem(configurationItem: ConfigurationItem) : Configuration =
        let (Configuration items) = this
        Configuration <| items @ [ configurationItem ]

    member this.append(config: Configuration) =
        let (Configuration items) = this
        let (Configuration items') = config
        Configuration <| items @ items'

type Builder() =
    member __.Yield(()) = Configuration.empty

    member __.Yield(configurationItem: ConfigurationItem) =
        Configuration.singleton configurationItem

    member __.YieldFrom(config: Configuration) = config

    member __.For(config: Configuration, f: unit -> Configuration) = config.append <| f ()

    member __.Zero() = Configuration.empty

    member __.Delay(f: unit -> Configuration) = f ()

    member __.Combine(config: Configuration, config': Configuration) = config.append <| config'

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

    [<CustomOperation("build")>]
    member __.Build(config: Configuration, outputs: Build.Outputs, ruleName: string, inputs: Build.Inputs) =
        config.addItem (
            Build
            <| Build.create
                {| Outputs = outputs
                   RuleName = ruleName
                   Inputs = inputs |}
        )

    [<CustomOperation("build")>]
    member this.Build(config: Configuration, outputs: Build.Outputs, ruleName: string, inputs: list<string>) =
        let inputs = Build.Inputs(explicit = inputs, implicit = [])
        this.Build(config, outputs, ruleName, inputs)

    [<CustomOperation("build")>]
    member this.Build(config: Configuration, outputs: list<string>, ruleName: string, inputs: Build.Inputs) =
        let outputs = Build.Outputs(explicit = outputs, implicit = [])
        this.Build(config, outputs, ruleName, inputs)

    [<CustomOperation("build")>]
    member this.Build(config: Configuration, outputs: list<string>, ruleName: string, inputs: list<string>) =
        let outputs = Build.Outputs(explicit = outputs, implicit = [])
        let inputs = Build.Inputs(explicit = inputs, implicit = [])
        this.Build(config, outputs, ruleName, inputs)

let ninja = Builder()

type Ninja =
    static member generate(?filename: string) : Configuration -> unit =
        fun config ->
            let filename = filename |> Option.defaultValue "build.ninja"

            let (Configuration items) = config

            let content =
                items
                |> List.map (ConfigurationItem.display)
                |> String.concat "\n"

            File.WriteAllText(filename, content + "\n")

open System.Runtime.CompilerServices

[<Extension>]
type ExplicitOutputsExtension =
    [<Extension>]
    static member implicitOutput(this: list<string>, files: list<string>) : Build.Outputs =
        Build.Outputs(explicit = this, implicit = files)

    [<Extension>]
    static member implicitInput(this: list<string>, files: list<string>) : Build.Inputs =
        Build.Inputs(explicit = this, implicit = files)
