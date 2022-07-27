module NinjaFs.Example

open NinjaFs

let config = // DelayedConfiguration
    ninja {
        var "builddir" "build"

        rule "compile" "gcc -c -o $out $in"

        rule "link" "gcc -o $out $in"

        yield VarDecl(VarDecl.create {| Name = "foo"; Value = "bar" |})

        yield!
            if false then
                ninja { build [ "build/main.o" ] "compile" [ "main.c" ] }
            else
                ninja { build [ "build/main.o" ] "compile" ([ "main.c" ].implicitInput [ "lib.h" ]) }
    }

// printfn "config |> Ninja.generate ()"
config |> Ninja.generate ()

(*
__.Delay (fun () ->
    __.For (
        __.Rule (
            __.Rule (
                __.Var (
                    (__.Yield ())
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
            __.Combine (
                __.Delay (fun () ->
                    __.Yield (VarDecl(VarDecl.create {| Name = "foo"; Value = "bar" |}))
                )
                __.YieldFrom
                    (if false
                    then
                        __.Delay (fun () ->
                            __.Build (
                                (__.Yield ())
                                [ "main.o" ]
                                "compile"
                                [ "main.c" ]
                            )
                        )
                    else
                        __.Delay (fun () ->
                            __.Build (
                                (__.Yield ())
                                [ "main.o "]
                                "compile"
                                ([ "main.c" ].implicitInput [ "lib.h" ])
                            )
                        ))
                )
            )
    )
)
*)
