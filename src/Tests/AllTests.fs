[<RequireQualifiedAccess>]
module TubeDl.Tests.AllTests

open Expecto

open TubeDl.Tests

let tests = testList "all tests" [ HandleFiles.tests ; ParseSelection.tests ]
