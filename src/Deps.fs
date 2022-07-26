module NinjaFs.Deps

type T =
    | Gcc
    | Msvc

let display deps =
    match deps with
    | Gcc -> "gcc"
    | Msvc -> "msvc"
