module TubeDl.Download.Download

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu

open TubeDl.Cli
open TubeDl.Rich

let runDownload res =
    match CliArgParse.initCfgFromArgs res with
    | Error DownloadTypeMissing ->
        Markup.eprintn "Specify a download types with [italic]--video[/] or [italic]--channel[/]"
        Error ArgumentsNotSpecified
    | Error InvalidPath ->
        eprintfn "The given path should be absolute"
        Error ArgumentsNotSpecified
    | Ok cfg ->

    let printError e =
        Markup.eprintn $":collision: [bold red]Failure![/] %s{DownloadError.errorMsg cfg e}"

    match cfg.DownloadType with
    | DownloadType.Video id ->
        DownloadVideo.runDownloadVideo cfg id
        |> Async.RunSynchronously
        |> Result.teeError printError
        |> Result.mapError CliError.DownloadError
    | DownloadType.Channel id ->
        DownloadChannel.runDownloadChannel cfg id
        |> Async.RunSynchronously
        |> Result.teeError printError
        |> Result.mapError CliError.DownloadError
