module TubeDl.DownloadVideo

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu
open Spectre.Console

open TubeDl.Cli
open TubeDl.Rich

type VideoDownloadError =
    | ApiError of TubeInfoError
    | SaveFileError of SaveFileError

type FinishedStep =
    | Metadata
    | Download
    | FileHandling

let handleDownload reporter cfg video =
    asyncResult {
        let! path =
            TubeInfo.pathForVideo cfg.Token video.Id
            |> AsyncResult.mapError VideoDownloadError.ApiError

        reporter FinishedStep.Metadata

        let! stream =
            TubeInfo.downloadVideo cfg.Token path.Path
            |> AsyncResult.mapError VideoDownloadError.ApiError

        reporter FinishedStep.Download

        do!
            HandleFiles.saveVideo cfg.OverwriteFile cfg.Path video path stream
            |> AsyncResult.mapError VideoDownloadError.SaveFileError

        reporter FinishedStep.FileHandling

        return video.Title
    }

let handleDownloadFull reporter cfg id =
    asyncResult {
        let! videoInfo =
            TubeInfo.videoInfo cfg.Token id
            |> AsyncResult.mapError VideoDownloadError.ApiError

        return! handleDownload reporter cfg videoInfo
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
                    | FileHandling -> ()

                    StatusContext.setSpinner ctx Spinner.Known.Dots10
                    StatusContext.setStatus ctx "[yellow]Saving video[/]"

                return
                    handleDownloadFull showFinishedStep cfg id
                    |> Async.RunSynchronously
            }

        let! res = Status.startDefault "[yellow]Fetching video metadata[/]" callback

        match res with
        | Ok title ->
            Markup.printn
                $":popcorn: [bold green]Success![/] The video [bold]%s{title}[/] was downloaded to [underline]%s{cfg.Path}[/]"

            return Ok ()
        | Error e ->
            // TODO proper error print
            Markup.printn $":collision: [bold red]Failure![/] [red]The error [bold]%A{e}[/] occured[/]"
            return Error e
    }
