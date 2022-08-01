open Expecto

let tests =
    test "A simple test" {
        let subject = "Hello, world"
        Expect.equal subject "Hello, world" "The strings should be equal"
    }

[<EntryPoint>]
let main args =
    runTestsWithArgs defaultConfig args tests
