module TubeDl.Download.DownloadVideo

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu
open Spectre.Console

open TubeDl
open TubeDl.Cli
open TubeDl.Rich

let handleDownload reporter cfg video =
    asyncResult {
        let! path =
            TubeInfo.pathForVideo cfg.Token video.Id
            |> AsyncResult.mapError DownloadError.TubeInfoError

        reporter FinishedDlStep.Metadata

        let! stream =
            TubeInfo.downloadVideo cfg.Token path.Path
            |> AsyncResult.mapError DownloadError.TubeInfoError

        reporter FinishedDlStep.Download

        let! fullPath =
            HandleFiles.saveVideo cfg.OverwriteFile cfg.Path video path stream
            |> AsyncResult.mapError DownloadError.SaveFileError

        FinishedDlStep.FileHandling fullPath |> reporter

        return video.Title, fullPath
    }

let handleDownloadFull reporter cfg id =
    asyncResult {
        let! videoInfo =
            TubeInfo.videoInfo cfg.Token id
            |> AsyncResult.mapError DownloadError.TubeInfoError

        return! handleDownload reporter cfg videoInfo
    }

let runDownloadVideo cfg id =
    asyncResult {
        let callback ctx =
            taskResult {
                let showFinishedStep (step : FinishedDlStep) =
                    match step with
                    | FinishedDlStep.Metadata ->
                        Markup.log "Received video metadata"
                        StatusContext.setSpinner ctx Spinner.Known.BouncingBar
                        StatusContext.setStatus ctx "[bold blue]Fetching video file[/]"
                    | FinishedDlStep.Download -> Markup.log "Received video file"
                    | FinishedDlStep.FileHandling _ -> () // Download was finished -> command is finished so show nothing

                    StatusContext.setSpinner ctx Spinner.Known.Dots10
                    StatusContext.setStatus ctx "[yellow]Saving video[/]"

                return! handleDownloadFull showFinishedStep cfg id
            }

        let! videoTitle, FullPath path = Status.startDefault "[yellow]Fetching video metadata[/]" callback

        $":popcorn: [bold green]Success![/] The video [bold]%s{videoTitle}[/] was downloaded to [italic]%s{path}[/]"
        |> Markup.printn

        return ()
    }
