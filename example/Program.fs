module NinjaFs.Example

open NinjaFs

ninja {
    var "builddir" "build"

    rule "compile" "gcc -c -o $out $in"

    rule "link" "gcc -o $out $in"

    yield!
        if false then
            ninja { build [ "build/main.o" ] "compile" [ "main.c" ] }
        else
            ninja { build [ "build/main.o" ] "compile" ([ "main.c" ].implicitInput [ "lib.h" ]) }
}
|> Ninja.generate ()

(*
    __.For (
        __.Rule (
            __.Rule (
                __.Var (
                    (__.Yield null)
                    "builddir"
                    "build"
                )
                "compile"
                "gcc -c -o $out $in"
            )
            "link"
            "gcc -o $out $in"
        )
        fun () ->
            if false
            then
                __.YieldFrom (
                    __.Build (
                        (__.Yield null)
                        [ "build/main.o" ]
                        "compile"
                        [ "main.c" ]
                    )
                )
            else
                __.YieldFrom (
                    __.Build (
                        (__.Yield null)
                        [ "build/main.o" ]
                        "compile"
                        ([ "main.c" ].implicitInput [ "lib.h" ])
                    )
                )
    )
*)
