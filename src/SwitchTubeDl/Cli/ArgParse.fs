namespace TubeDl.Cli

open Argu
open FsToolkit.ErrorHandling

open TubeDl
open TubeDl.Cli

[<RequireQualifiedAccess>]
type DownloadType =
    | Channel of Id : string
    | Video of Id : string

[<RequireQualifiedAccess>]
type ChannelFilter = | All

type TokenParseResult =
    | Ask
    | Provided of Api.Token

module TokenParseResult =
    let unsafeExtract =
        function
        | Ask -> failwith "Please provide a valid token"
        | Provided token -> token

type CliCfg =
    {
        DownloadType : DownloadType
        TokenParseResult : TokenParseResult
        Path : string
        ExistingFileHandling : ExistingFilesHandling
        ChannelFilter : ChannelFilter option
    }

type CompleteCfg =
    {
        DownloadType : DownloadType
        Token : Api.Token
        Path : string
        ExistingFileHandling : ExistingFilesHandling
        ChannelFilter : ChannelFilter option
    }

module CompleteCfg =
    let unsafeFromCliCfg (cliCfg : CliCfg) =
        {
            DownloadType = cliCfg.DownloadType
            Token = TokenParseResult.unsafeExtract cliCfg.TokenParseResult
            Path = cliCfg.Path
            ExistingFileHandling = cliCfg.ExistingFileHandling
            ChannelFilter = cliCfg.ChannelFilter
        }

type CfgParseError =
    | InvalidPath
    | DownloadTypeMissing

// Alias for less noise
type ParseRes = ParseResults<CliArgs>

module CliArgParse =
    // If all download types would be their separate subcommands this wouldn't be required.
    // But that has a usability tradeoff for the user.
    // Especially as a specific help message currently can't be written to the subcommand help page using Argu.
    let tryGetDownloadType (results : ParseRes) =
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

    let tryGetPath (results : ParseRes) =
        match results.Contains CliArgs.Path with
        | true ->
            let path = results.GetResult CliArgs.Path

            match Path.isFullyQualified path with
            | true -> Ok path
            | false -> Error InvalidPath
        | false -> Env.workingDir |> Ok

    let tryGetChannelFilter (results : ParseRes) =
        // Return type is Result as Date Parsing will require Result
        let all = results.Contains CliArgs.All

        match all with
        | true -> Some ChannelFilter.All |> Ok
        | false -> None |> Ok

    let initCfgFromArgs results =
        result {
            let! dlType = tryGetDownloadType results

            let token =
                match results.Contains CliArgs.Token with
                | true ->
                    results.GetResult CliArgs.Token
                    |> Api.Token
                    |> TokenParseResult.Provided
                | false -> TokenParseResult.Ask

            let! path = tryGetPath results

            let existingFileHandling =
                let skip = results.Contains CliArgs.Skip
                let force = results.Contains CliArgs.Force

                match skip, force with
                | true, _ -> ExistingFilesHandling.Skip
                | _, true -> ExistingFilesHandling.Overwrite
                | false, false -> ExistingFilesHandling.KeepAsIs

            let! channelFilterOpt = tryGetChannelFilter results

            let cfg =
                {
                    DownloadType = dlType
                    TokenParseResult = token
                    Path = path
                    ExistingFileHandling = existingFileHandling
                    ChannelFilter = channelFilterOpt
                }

            return cfg
        }
