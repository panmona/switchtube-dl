module TubeDl.Download.DownloadVideo

open FsToolkit.ErrorHandling
open Spectre.Console

open TubeDl
open TubeDl.Cli
open TubeDl.Rich

let handleDownload cfg reporter video =
    asyncResult {
        let! path =
            TubeInfo.pathForVideo cfg.Token video.Id
            |> AsyncResult.mapError DownloadError.TubeInfoError

        reporter FinishedDlStep.Metadata

        let existingVideoOpt =
            HandleFiles.tryFindVideo cfg.ExistingFileHandling cfg.Path video path

        match existingVideoOpt with
        | Some fullPath ->
            let res = FileWriteResult.Skipped fullPath
            FinishedDlStep.FileHandling res |> reporter
            return video.Title, res
        | None ->
            let! stream =
                TubeInfo.downloadVideo cfg.Token path.Path
                |> AsyncResult.mapError DownloadError.TubeInfoError

            reporter FinishedDlStep.Download

            let! writeResult =
                HandleFiles.saveVideo cfg.ExistingFileHandling cfg.Path video path stream
                |> AsyncResult.mapError DownloadError.SaveFileError

            FinishedDlStep.FileHandling writeResult |> reporter

            return video.Title, writeResult
    }

let private handleDownloadFull reporter cfg id =
    asyncResult {
        let! videoInfo =
            TubeInfo.videoInfo cfg.Token id
            |> AsyncResult.mapError DownloadError.TubeInfoError

        return! handleDownload cfg reporter videoInfo
    }

let runDownload cfg id =
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

        let! videoTitle, saveFileSuccess = Status.startDefault "[yellow]Fetching video metadata[/]" callback

        match saveFileSuccess with
        | FileWriteResult.Written (FullPath path) ->
            $":popcorn: [bold green]Success![/] The video [bold]%s{esc path}[/] was downloaded to [italic]%s{esc path}[/]"
            |> Markup.printn
        | FileWriteResult.Skipped fullPath ->
            let fileName = FullPath.last fullPath
            // Don't delete the extra space! It is needed so that this specific emoji has a spacing to the next character.
            $":next_track_button:  [yellow bold]Skipped[/] saving of video \"[italic]%s{esc videoTitle}[/]\" as it already exists as \"[italic]%s{esc fileName}[/]\""
            |> Markup.printn

        return ()
    }
