namespace TubeDl.Cli

open Argu

[<RequireQualifiedAccess>]
type ConfigArgs =
    | [<AltCommandLine("-p")>] Path

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Path -> "Save path as default path for downloaded videos. Default is the working directory."

type DownloadArgs =
    | [<Unique ; First ; AltCommandLine("-c")>] Channel of channel_id : string
    | [<Unique ; First ; AltCommandLine("-v")>] Video of video_id : string
    | [<Unique ; First ; AltCommandLine("-u")>] Url of url : string
    | [<AltCommandLine("-t")>] Token of token : string
    | [<AltCommandLine("-p")>] Path of path : string
    | [<AltCommandLine("-f")>] Force

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Channel _ -> "Download videos from this channel. Starts in interactive mode if no date option is given"
            | Video _ -> "Download video"
            | Url _ ->
                "Download this video or this channel. Starts in interactive mode if a channel url and no date option is given."
            | Token _ -> $"SwitchTube Token (defaults to env $%s{Constants.tokenEnvName})"
            | Path _ -> "Paths to download videos to (defaults to current dir)"
            | Force -> "Overwrite already existing files"

type CliArgs =
    | [<Unique ; First ; CliPrefix(CliPrefix.None)>] Dl of ParseResults<DownloadArgs>
    | [<Unique ; CliPrefix(CliPrefix.None)>] Config of ParseResults<ConfigArgs>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Dl _ -> "Download videos"
            | Config _ -> "Manages configuration parameters"

// TODO add date options
// TODO implement all these different switches properly
