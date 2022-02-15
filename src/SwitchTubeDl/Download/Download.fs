module TubeDl.Download.Download

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu

open TubeDl.Cli
open TubeDl.Rich

let runDownload res =
    match CliArgParse.initCfgFromArgs res with
    | Error TokenMissing ->
        eprintfn "Specify the token to access SwitchTube. If you don't have one generate one at: https://tube.switch.ch/access_tokens"
        Error ArgumentsNotSpecified
    | Error DownloadTypeMissing ->
        Markup.eprintn "Specify a download type with [italic]--video[/] or [italic]--channel[/]"
        Error ArgumentsNotSpecified
    | Error InvalidPath ->
        eprintfn "The given path should be absolute"
        Error ArgumentsNotSpecified
    | Ok cfg ->

    let printError e =
        let errorMsg = DownloadError.errorMsg cfg e // This fun should escape content correctly as it also contains markup
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
