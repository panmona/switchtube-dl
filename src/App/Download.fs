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

    // TODO async expression and proper error handling
    match cfg.DownloadType with
    | DownloadType.Video id ->
        DownloadVideo.runDownloadVideo cfg id
        |> Async.RunSynchronously
        |> ignore
    | DownloadType.Channel id ->
        let res =
            DownloadChannel.runDownloadChannel cfg id
            |> Async.RunSynchronously
        printfn "%A" res

    Ok ()
