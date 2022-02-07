module TubeDl.Download

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu
open Spectre.Console

open TubeDl.Cli
open TubeDl.Int
open TubeDl.ParseSelection
open TubeDl.Rich

// TODO maybe unite this with VideoDownloadError? not sure yet whether channel needs more info
type ChannelDownloadError =
    | ApiError of TubeInfoError
    | SaveFileError of SaveFileError

type ChannelMetadata =
    {
        Name : string
        Description : string
        Videos : VideoDetails list
    }

let downloadChannelMetadata cfg id =
    asyncResult {
        let! name, desc = TubeInfo.channelInfo cfg.Token id

        let! videos = TubeInfo.channelVideos cfg.Token id

        return
            {
                Name = name
                Description = desc
                Videos = videos
            }
    }

let printMetadataTable metadata =
    let columns =
        [
            Table.Column.withAlign "Index" Table.Alignment.Center
            Table.Column.init "Title"
            Table.Column.init "Duration"
            Table.Column.withAlign "Date" Table.Alignment.Center
        ]

    let videoToRow v =
        [
            v.Title
            (VideoDetails.duration >> TimeSpan.hourMinuteSecond) v
            DateTime.isoString v.PublishedAt
        ]

    let rows =
        metadata.Videos
        |> List.sortBy (fun v -> v.PublishedAt)
        |> List.mapi (fun i v -> [ $"%3i{i + 1}" ] @ videoToRow v)

    Table.table TableBorder.Minimal columns rows

let getSelectionInputFromPrompt max =
    let validation input =
        let errorAnnouncement = ":collision: [bold red]Uh oh![/]"

        match isValidAndInAllowedRange (max, 1) input with
        | Valid -> Ok ()
        | InvalidTokens e ->
            let tokens =
                e
                |> List.map (fun e -> $"[italic]%s{e}[/]")
                |> String.concat ", "

            $"%s{errorAnnouncement} [bold]Wasn't able to parse your input.[/]\nThe following tokens were invalid: %s{tokens}"
            |> Error
        | MaxValue i -> Error $"%s{errorAnnouncement} The number %i{i} is higher than the allowed: %i{max}"
        | MinValue i -> Error $"%s{errorAnnouncement} The number %i{i} is lower than 0"

    TextPrompt.promptWithValidation "Download >" validation

let runDownloadChannel cfg id =
    asyncResult {
        let callback _ctx =
            task {
                return
                    downloadChannelMetadata cfg id
                    |> Async.RunSynchronously
            }

        let! metadata =
            Status.startDefault "[yellow]Fetching channel metadata[/]" callback
            |> AsyncResult.teeError (fun e ->
                Markup.printn $":collision: [red][bold]Failure![/] The error [bold]%A{e}[/] occured[/]"
            )
            |> AsyncResult.mapError ChannelDownloadError.ApiError

        Markup.printn $":sparkles: Found the following videos for channel [bold underline]%s{metadata.Name}[/]:"
        printMetadataTable metadata

        "[bold underline]Choose[/] which videos you want to download by specifying their [yellow bold underline]index[/]\n"
        + "Separate the entries with [yellow bold],[/] and use [yellow bold]-[/] to specify a range ([italic]e.g.: \"1, 3-7, 10\"[/])"
        |> Markup.printn

        // TODO uhm Ctrl+C after this doesn't work. also happens with plain C#/F# app Console.ReadLine...
        let selection =
            List.length metadata.Videos
            |> getSelectionInputFromPrompt

        let videoIndexes =
            selection
            |> ParseSelection.tryParseSelection
            |> function
                | Ok i -> i
                | Error e ->
                    // This won't happen as the results were validated beforehand
                    failwith $"Error in parsing the selection for tokens %A{e}"

        printfn "%A" videoIndexes

        return ()
    }

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
            Markup.printn
                $":popcorn: [bold green]Success![/] The video [bold]%s{title}[/] was downloaded to [underline]%s{cfg.Path}[/]"

            return Ok ()
        | Error e ->
            // TODO proper error print
            Markup.printn $":collision: [bold red]Failure![/] [red]The error [bold]%A{e}[/] occured[/]"
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
    | DownloadType.Channel id ->
        runDownloadChannel cfg id
        |> Async.RunSynchronously
        |> ignore

    Ok ()
