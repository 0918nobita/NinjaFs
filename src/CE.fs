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
    | DelayedConfiguration of (unit -> Configuration)

    static member empty = Configuration []

    static member singleton item = Configuration [ item ]

    member this.force() =
        match this with
        | Configuration items -> items
        | DelayedConfiguration delayedConfiguration -> (delayedConfiguration ()).force ()

    member this.addItem(configurationItem: ConfigurationItem) : Configuration =
        match this with
        | Configuration items -> Configuration <| items @ [ configurationItem ]
        | DelayedConfiguration f ->
            DelayedConfiguration
            <| fun () -> f().addItem configurationItem

    member this.append(config: Configuration) =
        match this, config with
        | Configuration items, Configuration items' -> Configuration <| items @ items'
        | Configuration _, DelayedConfiguration f ->
            DelayedConfiguration
            <| fun () -> this.append <| f ()
        | DelayedConfiguration f, Configuration _ ->
            DelayedConfiguration
            <| fun () -> f().append config
        | DelayedConfiguration f, DelayedConfiguration f' ->
            DelayedConfiguration
            <| fun () -> f().append <| f' ()

type Builder() =
    member __.Yield(()) =
        // printfn "yield (unit)"
        Configuration.empty

    member __.Yield(configurationItem: ConfigurationItem) =
        // printfn "yield (ConfigurationItem)"
        Configuration.singleton configurationItem

    member __.YieldFrom(config: Configuration) =
        // printfn "yieldFrom (%A)" config
        config

    member __.For(config: Configuration, f: unit -> Configuration) =
        // printfn "for"
        let r = config.append <| f ()
        // printfn "end for"
        r

    member __.Zero() =
        // printfn "zero"
        Configuration.empty

    member __.Delay(f: unit -> Configuration) =
        // printfn "delay"
        DelayedConfiguration <| fun () -> f ()

    member __.Combine(config: Configuration, config': Configuration) =
        // printfn "combine"
        config.append <| config'

    [<CustomOperation("var")>]
    member __.Var(config: Configuration, name, value) =
        // printfn "var"
        config.addItem (
            VarDecl
            <| VarDecl.create {| Name = name; Value = value |}
        )

    [<CustomOperation("rule")>]
    member __.Rule(config: Configuration, name: string, command: string) =
        // printfn "rule"
        config.addItem (
            Rule
            <| Rule.create {| Name = name; Command = command |}
        )

    [<CustomOperation("build")>]
    member __.Build(config: Configuration, outputs: Build.Outputs, ruleName: string, inputs: Build.Inputs) =
        // printfn "build"
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

            let content =
                config.force ()
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
