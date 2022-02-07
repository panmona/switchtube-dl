namespace TubeDl.Cli

open Argu

open TubeDl
open TubeDl.Cli
open TubeDl.HandleFiles

[<RequireQualifiedAccess>]
type ExecutionType =
    | Config of ParseResults<ConfigArgs>
    | Download of ParseResults<DownloadArgs>

module CliArgParse =
    let tryGetExecutionType (results : ParseResults<CliArgs>) =
        let cfg = results.Contains CliArgs.Config
        let dl = results.Contains CliArgs.Dl

        match cfg, dl with
        | true, false ->
            results.GetResult CliArgs.Config
            |> ExecutionType.Config
            |> Some
        | false, true ->
            results.GetResult CliArgs.Dl
            |> ExecutionType.Download
            |> Some
        | _ -> None

[<RequireQualifiedAccess>]
type DownloadType =
    | Channel of Id: string
    | Video of Id: string
    | Url of Url: string

type DownloadCmdCfg =
    {
        DownloadType : DownloadType
        Token : Api.Token
        Path : string
        OverwriteFile : OverwriteFile
    }

type DownloadCmdCfgParseResult =
    | Success of DownloadCmdCfg
    | DownloadTypeMissing
    | InvalidPath

module DownloadArgParse =
    // If all of these would be their separate subcommands this wouldn't be required.
    // But that has a usability tradeoff for the user.
    // Especially as a specific help message currently can't be written to subcommand.
    let tryGetDownloadType (results : ParseResults<DownloadArgs>) =
        let channel = results.Contains DownloadArgs.Channel
        let video = results.Contains DownloadArgs.Video
        let url = results.Contains DownloadArgs.Url

        match video, channel, url with
        | true, _, _ ->
            results.GetResult DownloadArgs.Video
            |> DownloadType.Video
            |> Some
        | _, true, _ ->
            results.GetResult DownloadArgs.Channel
            |> DownloadType.Channel
            |> Some
        | _, _, true ->
            results.GetResult DownloadArgs.Url
            |> DownloadType.Url
            |> Some
        | false, false, false -> None

    let tryGetPath (results : ParseResults<DownloadArgs>) =
        match results.Contains DownloadArgs.Path with
        | true ->
            let path = results.GetResult DownloadArgs.Path

            match Path.isFullyQualified path with
            | true -> Some path
            | false -> None
        | false -> Env.workingDir |> Some

    let initCfgFromArgs results =
        let dlTypeOpt = tryGetDownloadType results
        let token = results.GetResult DownloadArgs.Token // Mandatory argument, safe to call this.
        let pathOpt = tryGetPath results

        let overwrite =
            match results.Contains DownloadArgs.Force with
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
