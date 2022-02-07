module TubeDl.DownloadCmd

open System.Diagnostics
open FsToolkit.ErrorHandling
open Spectre.Console

open TubeDl.Cli

let runDownloadUrl cfg url = ()

let runDownloadChannel cfg id = ()

type VideoDownloadError =
    | ApiError of TubeInfoError
    | SaveFileError of SaveFileError

// TODO make this handling a bit more beautiful
type Step =
    | VideoDetails
    | Download of bool // TODO start/finish type
    | FileHandling

// TODO how to name this?
let downloadVideo callback cfg id =
    asyncResult {
        let v = TubeInfo.videoInfo cfg.Token id
        let p = TubeInfo.pathForVideo cfg.Token id

        let! videoInfo =
            v
            |> AsyncResult.mapError VideoDownloadError.ApiError

        let! path =
            p
            |> AsyncResult.mapError VideoDownloadError.ApiError

        callback Step.VideoDetails
        callback (Step.Download false)

        let! stream =
            TubeInfo.downloadVideo cfg.Token path.Path
            |> AsyncResult.mapError VideoDownloadError.ApiError

        callback (Step.Download true)
        callback Step.FileHandling

        do!
            HandleFiles.saveVideo cfg.OverwriteFile cfg.Path videoInfo path stream
            |> AsyncResult.mapError VideoDownloadError.SaveFileError

        return videoInfo.Title
    }

let runDownloadVideo cfg id =
    async {
        // TODO create a small wrapper that makes this handling less verbose
        let! res =
            AnsiConsole
                .Status()
                .StartAsync(
                    "[yellow]Fetching video metadata[/]",
                    (fun ctx ->
                        task {
                            StatusContext.spinner ctx Spinner.Known.Default

                            let callback (step: Step) =
                                match step with
                                | VideoDetails ->
                                    AnsiConsole.MarkupLine("[grey50]LOG:[/] Received video metadata[grey50]...[/]")
                                | Download false ->
                                    StatusContext.spinner ctx Spinner.Known.BouncingBar
                                    StatusContext.status ctx "[bold blue]Fetching video file[/]"
                                | Download true ->
                                    AnsiConsole.MarkupLine("[grey50]LOG:[/] Received video file[grey50]...[/]")
                                | FileHandling ->
                                    StatusContext.spinner ctx Spinner.Known.Dots10
                                    StatusContext.status ctx "[yellow]Saving video[/]"

                            return
                                downloadVideo callback cfg id
                                |> Async.RunSynchronously
                        })
                )
            |> Async.AwaitTask

        match res with
        | Ok title ->
            $":popcorn: [bold green]Success![/] The video [bold]%s{title}[/] was downloaded to [underline]%s{cfg.Path}[/]"
            |> AnsiConsole.MarkupLine
        | Error e ->
            $":collision: [bold red]Failure![/] [red] The error [bold]%A{e}[/] occured[/]"
            |> AnsiConsole.MarkupLine
    }

let runDownload res =
    let cfg = DownloadArgParse.initCfgFromArgs res
    // TODO use Spectre.Console for proper formatting
    match cfg with
    | DownloadTypeMissing ->
        printfn "Specify one of the different download types with --video, --channel or --url"
        Error(ArgumentsNotSpecified "")
    | InvalidPath ->
        printfn "The configured path should be absolute"
        Error(ArgumentsNotSpecified "")
    | Success cfg ->

        // TODO async expression
        match cfg.DownloadType with
        | DownloadType.Video id -> runDownloadVideo cfg id |> Async.RunSynchronously
        | DownloadType.Channel id -> runDownloadChannel cfg id
        | DownloadType.Url url -> runDownloadUrl cfg url

        Ok()
