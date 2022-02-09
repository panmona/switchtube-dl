module TubeDl.Download

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu
open Spectre.Console

open TubeDl.Cli
open TubeDl.Int
open TubeDl.ParseSelection
open TubeDl.Rich

let runDownload res =
    let cfg = CliArgParse.initCfgFromArgs res
    // TODO use Spectre.Console for proper formatting
    match cfg with
    | DownloadTypeMissing ->
        eprintfn "Specify one of the different download types with --video, --channel or --url"
        Error (ArgumentsNotSpecified "")
    | InvalidPath ->
        eprintfn "The configured path should be absolute"
        Error (ArgumentsNotSpecified "")
    | Success cfg ->

    // TODO test if given path is writable!

    // TODO async expression and proper error handling
    let errorLog e =
        Markup.printn $":collision: [bold red]Failure![/] [red]The error [bold]%A{e}[/] occured[/]"
    match cfg.DownloadType with
    | DownloadType.Video id ->
        DownloadVideo.runDownloadVideo cfg id
        |> Async.RunSynchronously
        |> Result.teeError errorLog
        |> ignore
    | DownloadType.Channel id ->
        DownloadChannel.runDownloadChannel cfg id
        |> Async.RunSynchronously
        |> Result.teeError errorLog
        |> ignore

    Ok ()
