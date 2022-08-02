# NinjaFs: A toolkit for generating build.ninja from F# (script)

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
