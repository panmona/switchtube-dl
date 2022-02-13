module TubeDl.Tests.HandleFiles

open Expecto

open TubeDl

let private fileNameTests =
    testList
        "fileName"
        [
            testCase "Handles invalid & suboptimal chars"
            <| fun _ ->
                let video =
                    {
                        Id = "TheId"
                        ProfileId = 42
                        ChannelId = 69
                        Title = "A really cool video - about äüö|*.?:/\\"
                        Episode = None
                        PublishedAt = DateTime.now
                        LicenseCode = "Copyright"
                        DurationInMilliseconds = 15000
                    }

                let videoPath =
                    {
                        Path = "/some/path/to/file"
                        Name = "a name"
                        MediaType = MediaType.Mp4
                        ExpiresAt = DateTime.now
                    }

                let fileName = HandleFiles.fileName video videoPath

                Expect.equal fileName "A_really_cool_video_about_auo_TheId.mp4" "Didn't handle suboptimal chars"
        ]


let private fullPathTests =
    testList
        "fullPath"
        [
            testCase "Appends base path"
            <| fun _ ->
                let base' = "/my/base/path"
                let fileName = "a_file.mp4"
                let (FullPath path) = HandleFiles.fullPath base' fileName

                Expect.equal path "/my/base/path/a_file.mp4" "Didn't combine Path correctly"
        ]

let tests = testList "Handle Files" [ fileNameTests ; fullPathTests ]
