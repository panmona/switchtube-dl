namespace TubeDl.Cli

open Argu

open TubeDl
open TubeDl.Cli

[<RequireQualifiedAccess>]
type DownloadType =
    | Channel of Id: string
    | Video of Id: string

type CliCfg =
    {
        DownloadType : DownloadType
        Token : Api.Token
        Path : string
        OverwriteFile : OverwriteFile
    }

type CfgParseResult =
    | Success of CliCfg
    | DownloadTypeMissing
    | InvalidPath

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
            |> Some
        | _, true ->
            results.GetResult CliArgs.Channel
            |> DownloadType.Channel
            |> Some
        | false, false -> None

    let tryGetPath (results : ParseResults<CliArgs>) =
        match results.Contains CliArgs.Path with
        | true ->
            let path = results.GetResult CliArgs.Path

            match Path.isFullyQualified path with
            | true -> Some path
            | false -> None
        | false -> Env.workingDir |> Some

    let initCfgFromArgs results =
        // TODO maybe use result {} computation expression
        let dlTypeOpt = tryGetDownloadType results
        let token = results.GetResult CliArgs.Token // Mandatory argument, safe to call this.
        let pathOpt = tryGetPath results

        let overwrite =
            match results.Contains CliArgs.Force with
            | true -> OverwriteFile.Overwrite
            | false -> OverwriteFile.KeepAsIs

        match dlTypeOpt, pathOpt with
        | Some dlType, Some pathOpt ->
            {
                DownloadType = dlType
                Token = Api.Token token
                Path = pathOpt
                OverwriteFile = overwrite
            }
            |> Success
        | None, _ ->
            DownloadTypeMissing
        | _, None ->
            InvalidPath
