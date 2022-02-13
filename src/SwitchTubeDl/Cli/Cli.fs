namespace TubeDl.Cli

open Argu

type CliArgs =
    | [<Unique ; AltCommandLine("-v")>] Video of video_id : string
    | [<Unique ; AltCommandLine("-c")>] Channel of channel_id : string
    | [<ExactlyOnce ; AltCommandLine("-t")>] Token of token : string
    | [<AltCommandLine("-p")>] Path of path : string
    | [<AltCommandLine("-f")>] Force

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Video _ -> "Download type. Downloads a specific video. Prioritized if multiple download types are given"
            | Channel _ -> "Download type. Download videos from this channel." // TODO Starts in interactive mode if no date option is given
            | Token _ ->
                "Token to access the SwitchTube API (mandatory). Generate a token at https://tube.switch.ch/access_tokens"
            | Path _ -> "Paths to download videos to (defaults to current dir)"
            | Force -> "Overwrite already existing files"

// TODO add date options
// TODO add -a, --skip
