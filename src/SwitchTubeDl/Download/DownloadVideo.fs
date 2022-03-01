module TubeDl.Download.DownloadVideo

open FsToolkit.ErrorHandling
open Spectre.Console

open TubeDl
open TubeDl.Cli
open TubeDl.Rich

let private shouldSkipDownload cfg videoDetails videoPath =
    let (FullPath fullPath) =
        HandleFiles.fullPathForVideo cfg.Path videoDetails videoPath

    cfg.ExistingFileHandling = ExistingFilesHandling.Skip
    && File.exists fullPath

let handleDownload reporter cfg video =
    asyncResult {
        let! path =
            TubeInfo.pathForVideo cfg.Token video.Id
            |> AsyncResult.mapError DownloadError.TubeInfoError

        reporter FinishedDlStep.Metadata

        if shouldSkipDownload cfg video path then
            let res = FileWriteResult.Skipped
            FinishedDlStep.FileHandling res |> reporter
            return video.Title, res
        else
            let! stream =
                TubeInfo.downloadVideo cfg.Token path.Path
                |> AsyncResult.mapError DownloadError.TubeInfoError

            reporter FinishedDlStep.Download

            let! writeResult =
                HandleFiles.saveVideo cfg.ExistingFileHandling cfg.Path video path stream
                |> AsyncResult.mapError DownloadError.SaveFileError

            FinishedDlStep.FileHandling writeResult
            |> reporter

            return video.Title, writeResult
    }

let private handleDownloadFull reporter cfg id =
    asyncResult {
        let! videoInfo =
            TubeInfo.videoInfo cfg.Token id
            |> AsyncResult.mapError DownloadError.TubeInfoError

        return! handleDownload reporter cfg videoInfo
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
        | FileWriteResult.Skipped ->
            // Don't delete the extra space! It is needed so that this specific emoji has a spacing to the next character.
            $":next_track_button:  [yellow bold]Skipped[/] saving of video \"[italic]%s{esc videoTitle}[/]\" as it already exists"
            |> Markup.printn

        return ()
    }
