module TubeDl.Tests.Runner

open Expecto

[<EntryPoint>]
let main args =
    try
        runTestsWithArgs
            { defaultConfig with
                runInParallel = true
            }
            args
            AllTests.tests
    with
    | e ->
        printfn "Error: %s" e.Message
        -1
