namespace TubeDl.Cli

open Argu
open FsToolkit.ErrorHandling

open TubeDl
open TubeDl.Cli

[<RequireQualifiedAccess>]
type DownloadType =
    | Channel of Id : string
    | Video of Id : string

type CliCfg =
    {
        DownloadType : DownloadType
        Token : Api.Token
        Path : string
        OverwriteFile : OverwriteFile
    }

type CfgParseError =
    | InvalidPath
    | DownloadTypeMissing

module CliArgParse =
    // If all of these would be their separate subcommands this wouldn't be required.
    // But that has a usability tradeoff for the user.
    // Especially as a specific help message currently can't be written to subcommand.
    let tryGetDownloadType (results : ParseResults<CliArgs>) =
        let channel = results.Contains CliArgs.Channel
        let video = results.Contains CliArgs.Video

        match video, channel with
        | true, _ ->
            results.GetResult CliArgs.Video
            |> DownloadType.Video
            |> Ok
        | _, true ->
            results.GetResult CliArgs.Channel
            |> DownloadType.Channel
            |> Ok
        | false, false -> Error DownloadTypeMissing

    let tryGetPath (results : ParseResults<CliArgs>) =
        match results.Contains CliArgs.Path with
        | true ->
            let path = results.GetResult CliArgs.Path

            match Path.isFullyQualified path with
            | true -> Ok path
            | false -> Error InvalidPath
        | false -> Env.workingDir |> Ok

    let initCfgFromArgs results =
        result {
            let! dlType = tryGetDownloadType results
            let token = results.GetResult CliArgs.Token // Mandatory argument, safe to call this.
            let! path = tryGetPath results

            let overwrite =
                match results.Contains CliArgs.Force with
                | true -> OverwriteFile.Overwrite
                | false -> OverwriteFile.KeepAsIs

            let cfg =
                {
                    DownloadType = dlType
                    Token = Api.Token token
                    Path = path
                    OverwriteFile = overwrite
                }

            return cfg
        }
