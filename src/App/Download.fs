module TubeDl.Download

open System.Diagnostics
open FsToolkit.ErrorHandling
open Spectre.Console

open TubeDl.Cli
open TubeDl.Rich

let runDownloadChannel cfg id = ()

// TODO move to separate module later? leave as is for now
type VideoDownloadError =
    | ApiError of TubeInfoError
    | SaveFileError of SaveFileError

type FinishedStep =
    | Metadata
    | Download

// TODO how to name this?
let downloadVideo reportFinishedStep cfg id =
    asyncResult {
        let! videoInfo =
            TubeInfo.videoInfo cfg.Token id
            |> AsyncResult.mapError VideoDownloadError.ApiError

        let! path =
            TubeInfo.pathForVideo cfg.Token id
            |> AsyncResult.mapError VideoDownloadError.ApiError

        reportFinishedStep FinishedStep.Metadata

        let! stream =
            TubeInfo.downloadVideo cfg.Token path.Path
            |> AsyncResult.mapError VideoDownloadError.ApiError

        reportFinishedStep FinishedStep.Download

        do!
            HandleFiles.saveVideo cfg.OverwriteFile cfg.Path videoInfo path stream
            |> AsyncResult.mapError VideoDownloadError.SaveFileError

        return videoInfo.Title
    }

let runDownloadVideo cfg id =
    async {
        let callback ctx =
            task {
                let showFinishedStep (step : FinishedStep) =
                    match step with
                    | Metadata ->
                        Markup.log "Received video metadata"
                        StatusContext.setSpinner ctx Spinner.Known.BouncingBar
                        StatusContext.setStatus ctx "[bold blue]Fetching video file[/]"
                    | Download -> Markup.log "Received video file"

                    StatusContext.setSpinner ctx Spinner.Known.Dots10
                    StatusContext.setStatus ctx "[yellow]Saving video[/]"

                return
                    downloadVideo showFinishedStep cfg id
                    |> Async.RunSynchronously
            }

        let! res = Status.startDefault "[yellow]Fetching video metadata[/]" callback

        match res with
        | Ok title ->
            Markup.print
                $":popcorn: [bold green]Success![/] The video [bold]%s{title}[/] was downloaded to [underline]%s{cfg.Path}[/]"

            return Ok ()
        | Error e ->
            // TODO proper error print
            Markup.print $":collision: [bold red]Failure![/] [red]The error [bold]%A{e}[/] occured[/]"
            return Error e
    }

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
        runDownloadVideo cfg id
        |> Async.RunSynchronously
        |> ignore
    | DownloadType.Channel id -> runDownloadChannel cfg id

    Ok ()
