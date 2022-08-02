# NinjaFs

[![NuGet Badge](https://buildstats.info/nuget/NinjaFs)](https://www.nuget.org/packages/NinjaFs/)  [![Docs](https://github.com/0918nobita/NinjaFs/actions/workflows/docs.yml/badge.svg)](https://0918nobita.github.io/NinjaFs)

A toolkit for generating `build.ninja` from F# script

```fsharp
#r "nuget: NinjaFs"

open NinjaFs

ninja {
    var "builddir" "build"

    rule "compile" "gcc -c -o $out $in"

    build [ "build/main.o" ] "compile" [ "src/main.c" ]
}
|> Ninja.generate()
```

## Development

### Install dependencies

```bash
dotnet restore
dotnet tool restore
```

### Example


```bash
dotnet run --project example
```

### Generate API documentation

```bash
dotnet build
dotnet fsdocs build # or `dotnet fsdocs watch`
```
