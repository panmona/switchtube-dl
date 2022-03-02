namespace TubeDl.Cli

open Argu

type CliArgs =
    | [<Unique ; AltCommandLine("-v")>] Video of video_id : string
    | [<Unique ; AltCommandLine("-c")>] Channel of channel_id : string
    | [<AltCommandLine("-t")>] Token of token : string
    | [<AltCommandLine("-p")>] Path of path : string
    | [<AltCommandLine("-s")>] Skip
    | [<AltCommandLine("-f")>] Force
    | [<AltCommandLine("-a")>] All
    | Version

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Video _ -> "Download type. Downloads a specific video. Prioritized if multiple download types are given"
            | Channel _ ->
                "Download type. Download videos from this channel. Starts in interactive mode if no filter option is given"
            | Token _ -> "Token to access the SwitchTube API. Generate a token at https://tube.switch.ch/access_tokens"
            | Path _ -> "Paths to download videos to (defaults to current dir). The path must already exist."
            | Skip ->
                "Existing file handling option. Skip download of already existing files. Prioritized if multiple existing file options are given"
            | Force -> "Existing file handling option. Overwrite already existing files"
            | All -> "Filter option. Downloads all videos in a channel"
            | Version -> "Display the current version."

// TODO add date options
