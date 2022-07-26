module NinjaFs.VarDecl

type T = private { Name: string; Value: string }

let create (desc: {| Name: string; Value: string |}) =
    { Name = desc.Name; Value = desc.Value }

let display varDecl = $"%s{varDecl.Name} = %s{varDecl.Value}"
