module NinjaFs.Build

type Outputs = Outputs of explicit: list<string> * implicit: list<string>

module Outputs =
    let display outputs =
        let (Outputs (explicit, implicit)) = outputs
        let explicit = String.concat "" explicit

        let implicit =
            (if List.isEmpty implicit then
                 ""
             else
                 " | ")
            + String.concat "" implicit

        explicit + implicit

type Inputs = Inputs of explicit: list<string> * implicit: list<string>

module Inputs =
    let display inputs =
        let (Inputs (explicit, implicit)) = inputs
        let explicit = String.concat "" explicit

        let implicit =
            (if List.isEmpty implicit then
                 ""
             else
                 " | ")
            + String.concat "" implicit

        explicit + implicit

type T =
    private
        { RuleName: string
          Outputs: Outputs
          Inputs: Inputs }

let create
    (desc: {| RuleName: string
              Outputs: Outputs
              Inputs: Inputs |})
    =
    { RuleName = desc.RuleName
      Outputs = desc.Outputs
      Inputs = desc.Inputs }

let display build =
    let outputs = Outputs.display build.Outputs

    let inputs = Inputs.display build.Inputs

    $"build %s{outputs}: %s{build.RuleName} %s{inputs}"
