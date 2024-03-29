[<RequireQualifiedAccess>]
module TubeDl.Download.Download

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu

open TubeDl
open TubeDl.Cli
open TubeDl.Rich

let private withToken cfg =
    let providedToken =
        match cfg.TokenParseResult with
        | TokenParseResult.Ask ->
            Markup.printn "Please specify the [bold green]token[/] for accessing SwitchTube."
            Markup.printn "If you don't have one generate one at [italic]https://tube.switch.ch/access_tokens[/]"
            let input = TextPrompt.secretPrompt "> "

            input |> Token |> TokenParseResult.Provided
        | TokenParseResult.Provided _ as p -> p

    { cfg with
        TokenParseResult = providedToken
    }

let runDownload res =
    match CliArgParse.initCfgFromArgs res with
    | Error CfgParseError.DownloadTypeMissing ->
        Markup.eprintn "Specify a download type with [italic]--video[/] or [italic]--channel[/]"
        Error CliError.ArgumentsNotSpecified
    | Error CfgParseError.InvalidPath ->
        eprintfn "The given path should be absolute"
        Error CliError.ArgumentsNotSpecified
    | Ok cliCfg ->

    let cfg = withToken cliCfg |> CompleteCfg.unsafeFromCliCfg

    let printError e =
        let errorMsg = DownloadError.errorMsg cfg e // This fun should escape its content correctly as it also contains markup
        Markup.eprintn $":collision: [bold red]Failure![/] %s{errorMsg}"

    match cfg.DownloadType with
    | DownloadType.Video id ->
        DownloadVideo.runDownload cfg id
        |> Async.RunSynchronously
        |> Result.teeError printError
        |> Result.mapError CliError.DownloadError
    | DownloadType.Channel id ->
        DownloadChannel.runDownload cfg id
        |> Async.RunSynchronously
        |> Result.teeError printError
        |> Result.mapError CliError.DownloadError
