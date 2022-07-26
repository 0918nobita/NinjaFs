module NinjaFs.Build

type ExplicitOutputs = ExplicitOutputs of list<string>

module ExplicitOutputs =
    let display (ExplicitOutputs explicitOutputs) = String.concat "" explicitOutputs

type ImplicitOutputs = ImplicitOutputs of list<string>

module ImplicitOutputs =
    let display (ImplicitOutputs implicitOutputs) =
        (if List.isEmpty implicitOutputs then
             ""
         else
             " | ")
        + String.concat "" implicitOutputs

type ExplicitInputs = ExplicitInputs of list<string>

module ExplicitInputs =
    let display (ExplicitInputs explicitInputs) = String.concat "" explicitInputs

type ImplicitInputs = ImplicitInputs of list<string>

module ImplicitInputs =
    let display (ImplicitInputs implicitInputs) =
        (if List.isEmpty implicitInputs then
             ""
         else
             " | ")
        + String.concat "" implicitInputs

type T =
    private
        { RuleName: string
          Outputs: ExplicitOutputs * ImplicitOutputs
          Inputs: ExplicitInputs * ImplicitInputs }

let create
    (desc: {| RuleName: string
              Outputs: ExplicitOutputs * ImplicitOutputs
              Inputs: ExplicitInputs * ImplicitInputs |})
    =
    { RuleName = desc.RuleName
      Outputs = desc.Outputs
      Inputs = desc.Inputs }

let display build =
    let (explicitOutputs, implicitOutputs) = build.Outputs

    let explicitOutputs = ExplicitOutputs.display explicitOutputs

    let implicitOutputs = ImplicitOutputs.display implicitOutputs

    let (explicitInputs, implicitInputs) = build.Inputs

    let explicitInputs = ExplicitInputs.display explicitInputs

    let implicitInputs = ImplicitInputs.display implicitInputs

    $"build %s{explicitOutputs}%s{implicitOutputs}: %s{build.RuleName} %s{explicitInputs}%s{implicitInputs}"
