namespace TubeDl.Cli

open Argu

type CliArgs =
    | [<Unique ; AltCommandLine("-v")>] Video of video_id : string
    | [<Unique ; AltCommandLine("-c")>] Channel of channel_id : string
    | [<ExactlyOnce ; AltCommandLine("-t")>] Token of token : string
    | [<AltCommandLine("-p")>] Path of path : string
    | Skip
    | [<AltCommandLine("-f")>] Force
    | [<CliPrefix(CliPrefix.Dash)>] A

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Video _ -> "Download type. Downloads a specific video. Prioritized if multiple download types are given"
            | Channel _ ->
                "Download type. Download videos from this channel. Starts in interactive mode if no filter option is given"
            | Token _ ->
                "Token to access the SwitchTube API (mandatory). Generate a token at https://tube.switch.ch/access_tokens"
            | Path _ -> "Paths to download videos to (defaults to current dir)"
            | Skip ->
                "Existing file handling option. Skip saving of already existing files. Prioritized if multiple existing file options are given"
            | Force -> "Existing file handling option. Overwrite already existing files"
            | A -> "Filter option. Downloads all videos in a channel"

// TODO add date options
