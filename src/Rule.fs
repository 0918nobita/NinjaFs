module NinjaFs.Rule

type T =
    private
        { Name: string
          Command: string
          DepFile: option<string>
          Deps: option<Deps.T> }

let withDepfile
    (desc: {| Name: string
              Command: string
              Depfile: string
              Deps: Deps.T |})
    =
    { Name = desc.Name
      Command = desc.Command
      DepFile = Some desc.Depfile
      Deps = Some desc.Deps }

let create (desc: {| Name: string; Command: string |}) =
    { Name = desc.Name
      Command = desc.Command
      DepFile = None
      Deps = None }

let display rule =
    let depfile =
        rule.DepFile
        |> Option.map (fun depfile -> $"    depfile = %O{depfile}\n")
        |> Option.defaultValue ""

    let deps =
        rule.Deps
        |> Option.map (fun deps -> $"    deps = %O{deps}\n")
        |> Option.defaultValue ""

    $"rule %s{rule.Name}\n    command = %s{rule.Command}\n%s{depfile}%s{deps}"
