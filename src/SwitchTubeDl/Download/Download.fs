module TubeDl.Download.Download

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu

open TubeDl.Cli
open TubeDl.Rich

let private withToken cfg =
    let providedToken =
        match cfg.TokenParseResult with
        | Ask ->
            Markup.printn "Please specify the [bold green]token[/] for accessing SwitchTube."
            Markup.printn "If you don't have one generate one at [italic]https://tube.switch.ch/access_tokens[/]"
            let input = TextPrompt.secretPrompt "> "

            input
            |> TubeDl.Api.Token
            |> TokenParseResult.Provided
        | Provided t -> Provided t

    { cfg with
        TokenParseResult = providedToken
    }

let runDownload res =
    match CliArgParse.initCfgFromArgs res with
    | Error DownloadTypeMissing ->
        Markup.eprintn "Specify a download type with [italic]--video[/] or [italic]--channel[/]"
        Error ArgumentsNotSpecified
    | Error InvalidPath ->
        eprintfn "The given path should be absolute"
        Error ArgumentsNotSpecified
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
