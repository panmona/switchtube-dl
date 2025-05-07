[<RequireQualifiedAccess>]
module TubeDl.Tests.Runner

open Expecto

[<EntryPoint>]
let main args =
    try
        runTestsWithCLIArgs
            []
            args
            AllTests.tests
    with e ->
        printfn "Error: %s" e.Message
        -1
