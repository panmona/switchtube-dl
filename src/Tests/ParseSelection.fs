module TubeDl.Tests.ParseSelection

open Expecto

open TubeDl

let private tryParseSelectionTests =
    testList
        "tryParseSelection"
        [
            testCase "Returns all comma separated items"
            <| fun _ ->
                let items = "1,3,5,7,6,8,10"
                let parsedRes = ParseSelection.tryParseSelection items
                let parsedSel = Expect.wantOk parsedRes "Parsing failed"
                let expectedSel = [ 1 ; 3 ; 5 ; 7 ; 6 ; 8 ; 10 ]
                Expect.equal parsedSel expectedSel "Not the correct elements were returned"

            testCase "Removes duplicates"
            <| fun _ ->
                let items = "1,3,3,1"
                let parsedRes = ParseSelection.tryParseSelection items
                let parsedSel = Expect.wantOk parsedRes "Parsing failed"
                let expectedSel = [ 1 ; 3 ]
                Expect.equal parsedSel expectedSel "Not the correct elements were returned"

            testCase "Handles ranges"
            <| fun _ ->
                let items = "1-3,5-7"
                let parsedRes = ParseSelection.tryParseSelection items
                let parsedSel = Expect.wantOk parsedRes "Parsing failed"
                let expectedSel = [ 1 ; 2 ; 3 ; 5 ; 6 ; 7 ]
                Expect.equal parsedSel expectedSel "Not the correct elements were returned"

            testCase "Handles spaces"
            <| fun _ ->
                let items = "1, 3, 5, 9"
                let parsedRes = ParseSelection.tryParseSelection items
                let parsedSel = Expect.wantOk parsedRes "Parsing failed"
                let expectedSel = [ 1 ; 3 ; 5 ; 9 ]
                Expect.equal parsedSel expectedSel "Not the correct elements were returned"

            testCase "Returns all invalid tokens"
            <| fun _ ->
                let items = "x,y,,-"
                let parsedRes = ParseSelection.tryParseSelection items

                let invalidTokens = Expect.wantError parsedRes "Parsing failed"

                Expect.equal invalidTokens [ "x" ; "y" ; "" ; "-" ] "Not the correct invalid tokens were returned"
        ]

let private isValidAndInRangeTests =
    testList
        "isValidAndInRange"
        [
            testCase "Returns correct validity"
            <| fun _ ->
                Expect.equal
                    ParseSelection.Valid
                    (ParseSelection.isValidAndInRange (0, 10) "1,2,3")
                    "Selection wasn't determined to be valid"

                Expect.equal
                    (ParseSelection.InvalidTokens [ "x" ])
                    (ParseSelection.isValidAndInRange (0, 10) "x")
                    "Selection wasn't determined to be invalid"

            testCase "Handles min value"
            <| fun _ ->
                Expect.equal
                    ParseSelection.Valid
                    (ParseSelection.isValidAndInRange (3, 10) "3,5")
                    "<= wasn't handled correctly"

                Expect.equal
                    (ParseSelection.MinValue -1)
                    (ParseSelection.isValidAndInRange (0, 10) "-1")
                    "Selection didn't determine MinValue correctly"

                Expect.equal
                    (ParseSelection.MinValue 1)
                    (ParseSelection.isValidAndInRange (3, 10) "4,1,5")
                    "Selection didn't determine MinValue correctly"

            testCase "Handles max value"
            <| fun _ ->
                Expect.equal
                    ParseSelection.Valid
                    (ParseSelection.isValidAndInRange (0, 10) "3,10")
                    ">= wasn't handled correctly"

                Expect.equal
                    (ParseSelection.MaxValue 11)
                    (ParseSelection.isValidAndInRange (0, 10) "11")
                    "Selection didn't determine MaxValue correctly"
        ]

let tests =
    testList "Parse Selection" [ tryParseSelectionTests ; isValidAndInRangeTests ]
