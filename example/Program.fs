module NinjaFs.Example

open NinjaFs

(ninja {
    var "builddir" "build"

    rule "compile" "gcc -c -o $out $in"

    rule "link" "gcc -o $out $in"
})
    .generate ()
