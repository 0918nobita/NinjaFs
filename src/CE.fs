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

        File.WriteAllText(filename, content + "\n")

type OutputDescriptor =
    private
    | OutputDescriptor of explicit: list<string> * implicit: list<string>

    member this.implicit(files: list<string>) =
        let (OutputDescriptor (explicit, _)) = this
        OutputDescriptor(explicit = explicit, implicit = files)

let output (files: list<string>) =
    OutputDescriptor(explicit = files, implicit = [])

type InputDescriptor =
    private
    | InputDescriptor of explicit: list<string> * implicit: list<string>

    member this.implicit(files: list<string>) =
        let (InputDescriptor (explicit, _)) = this
        InputDescriptor(explicit = explicit, implicit = files)

let input (files: list<string>) =
    InputDescriptor(explicit = files, implicit = [])

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

    [<CustomOperation("build")>]
    member __.Build(config: Configuration, outputs: OutputDescriptor, ruleName: string, inputs: InputDescriptor) =
        let (OutputDescriptor (explicitOutputs, implicitOutputs)) = outputs
        let explicitOutputs = Build.ExplicitOutputs explicitOutputs
        let implicitOutputs = Build.ImplicitOutputs implicitOutputs

        let (InputDescriptor (explicitInputs, implicitInputs)) = inputs
        let explicitInputs = Build.ExplicitInputs explicitInputs
        let implicitInputs = Build.ImplicitInputs implicitInputs

        config.addItem (
            Build
            <| Build.create
                {| Outputs = (explicitOutputs, implicitOutputs)
                   RuleName = ruleName
                   Inputs = (explicitInputs, implicitInputs) |}
        )

let ninja = Builder()
