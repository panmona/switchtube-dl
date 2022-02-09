module TubeDl.DownloadVideo

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu
open Spectre.Console

open TubeDl.Cli
open TubeDl.Rich

type VideoDownloadError =
    | TubeInfoError of TubeInfoError
    | SaveFileError of SaveFileError

module VideoDownloadError =
    let (|ApiError|_|) = function
        | TubeInfoError (TubeInfoError.ApiError e) -> Some e
        | _ -> None

// TODO move elsewhere so that it can be nicely used from downloadChannel and downloadVideo
type FinishedStep =
    | Metadata
    | Download
    | FileHandling

let handleDownload reporter cfg video =
    asyncResult {
        let! path =
            TubeInfo.pathForVideo cfg.Token video.Id
            |> AsyncResult.mapError VideoDownloadError.TubeInfoError

        reporter FinishedStep.Metadata

        let! stream =
            TubeInfo.downloadVideo cfg.Token path.Path
            |> AsyncResult.mapError VideoDownloadError.TubeInfoError

        reporter FinishedStep.Download

        let! fileName =
            HandleFiles.saveVideo cfg.OverwriteFile cfg.Path video path stream
            |> AsyncResult.mapError VideoDownloadError.SaveFileError

        reporter FinishedStep.FileHandling

        return video.Title, fileName
    }

let handleDownloadFull reporter cfg id =
    asyncResult {
        let! videoInfo =
            TubeInfo.videoInfo cfg.Token id
            |> AsyncResult.mapError VideoDownloadError.TubeInfoError

        return! handleDownload reporter cfg videoInfo
    }

let runDownloadVideo cfg id =
    asyncResult {
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

        let! videoTitle, path = Status.startDefault "[yellow]Fetching video metadata[/]" callback

        Markup.printn
            $":popcorn: [bold green]Success![/] The video [bold]%s{videoTitle}[/] was downloaded to [italic]%s{path}[/]"

        return ()
    }
