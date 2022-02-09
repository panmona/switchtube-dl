module TubeDl.DownloadChannel

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu
open Spectre.Console

open TubeDl.Cli
open TubeDl.ParseSelection
open TubeDl.Rich

// TODO better unify error types if possible
type ChannelDownloadError =
    | TubeInfoError of TubeInfoError
    | SaveFileError of SaveFileError
    | VideoDownloadError of DownloadVideo.VideoDownloadError

module ChannelDownloadError =
    let (|ApiError|_|) = function
        | TubeInfoError (TubeInfoError.ApiError e)
        | VideoDownloadError (DownloadVideo.VideoDownloadError.ApiError e) -> Some e
        | _ -> None

    let errorMessage cfg error =
        match error with
        | ApiError Api.UnauthorizedAccess ->
            "A valid token needs to be provided."
        | ApiError Api.ResourceNotFound ->
            "An existing channel id needs to be provided."
        | ApiError Api.TooManyRequests
        | ApiError Api.ApiError ->
            "SwitchTube encountered an error, please try again later."
        | TubeInfoError (TubeInfoError.DecodeError s)
        | VideoDownloadError (DownloadVideo.VideoDownloadError.TubeInfoError (TubeInfoError.DecodeError s)) ->
            "There was an error while decoding the JSON received from the API.\n" +
            $"%s{GitHub.createIssue} with the following info:\n" +
            $"[italic]%s{s}[/]"
        | SaveFileError SaveFileError.AccessDenied ->
            $"Wasn't able to write a file to the path %s{cfg.Path}. Please ensure that the path is writable."
        | SaveFileError SaveFileError.FileExists ->
            $"The video exists already in the path %s{cfg.Path}. If you want to overwrite it, use the option [bold yellow]-f[/]."
        | SaveFileError (SaveFileError.InvalidPath path) ->
            $"Wasn't able to save the video to the following invalid path: [italic]%s{path}[/]." +
            $"If the name of the video file seems to be the cause: %s{GitHub.createIssue}."
        | SaveFileError SaveFileError.DirNotFound ->
            "The given path was invalid "
        | SaveFileError SaveFileError.IOError ->
            "There was an IO error while saving the file. Please check your path and try again."
        | e ->
            $"Wasn't able to correctly determine the error type of [bold red]%A{e}[/]. %s{GitHub.createIssue}."
type private ChannelMetadata =
    {
        Name : string
        Description : string
        Videos : VideoDetails list
    }

let private downloadChannelMetadata cfg id =
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

let private printMetadataTable metadata =
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

let private getSelectionInputFromPrompt max =
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

let private downloadVideos cfg logCallback (videos : VideoDetails list) =
    async {
        let handleDownload v =
            asyncResult {
                let prefilledCallback = logCallback v.Title
                return!
                    DownloadVideo.handleDownload prefilledCallback cfg v
                    |> AsyncResult.mapError ChannelDownloadError.VideoDownloadError
            }

        let! allRes =
            videos
            |> List.chunkBySize 3
            |> List.map (List.map handleDownload >> Async.parallelCombine)
            |> Async.sequential

        let folder state item =
            // Returns the first error that occured as error
            match state, item with
            | Ok _, Ok _ -> Ok ()
            | Error errs, _
            | _, Error errs -> Error errs
        // Map the results for proper error reporting to the parent
        return List.concat allRes |> List.fold folder (Ok ())
    }

let runDownloadChannel cfg id =
    asyncResult {
        let! metadata =
            let callback _ctx =
                task {
                    return
                        downloadChannelMetadata cfg id
                        |> Async.RunSynchronously
                }

            Status.startDefault "[yellow]Fetching channel metadata[/]" callback
            |> AsyncResult.teeError (fun e ->
                Markup.printn $":collision: [red][bold]Failure![/] The error [bold]%A{e}[/] occured[/]"
            )
            |> AsyncResult.mapError ChannelDownloadError.TubeInfoError

        Markup.printn $":sparkles: Found the following videos for channel [yellow bold underline]%s{metadata.Name}[/]:"
        printMetadataTable metadata

        "[bold underline]Choose[/] which videos you want to download by specifying their [yellow bold underline]index[/]\n"
        + "Separate the entries with [yellow bold],[/] and use [yellow bold]-[/] to specify a range ([italic]e.g.: \"1, 3-7, 10\"[/])"
        |> Markup.printn

        // TODO Ctrl+C after this doesn't work. also happens with plain C#/F# app Console.ReadLine...
        let selection =
            List.length metadata.Videos
            |> getSelectionInputFromPrompt

        let videoIndexes =
            selection
            |> tryParseSelection
            |> function
                | Ok sel -> List.map (fun i -> i - 1) sel
                | Error e ->
                    // This won't happen as the results were validated beforehand
                    failwith $"Error in parsing the selection for tokens %A{e}"

        let videosToDownload =
            metadata.Videos
            |> List.indexed
            |> List.filter (fun (i, _) -> List.contains i videoIndexes)
            |> List.map snd

        let callback _ctx =
            task {
                let showFinishedStep video (step : DownloadVideo.FinishedStep) =
                    match step with
                    | DownloadVideo.Metadata ->
                        Markup.log $"Received video [yellow bold]metadata[/] of \"[italic]%s{video}[/]\""
                    | DownloadVideo.Download -> Markup.log $"Received video [yellow bold]file[/] of \"[italic]%s{video}[/]\""
                    | DownloadVideo.FileHandling -> Markup.log $"[yellow bold]Saved file[/] of \"[italic]%s{video}[/]\""

                return
                    downloadVideos cfg showFinishedStep videosToDownload
                    |> Async.RunSynchronously
            }

        let! _ = Status.start Spinner.Known.BouncingBar $"[bold blue]Downloading videos of channel [yellow underline]%s{metadata.Name}[/][/]" callback

        Markup.printn
            $":popcorn: [bold green]Success![/] The requested videos of channel [yellow bold]%s{metadata.Name}[/] were downloaded to [italic]%s{cfg.Path}[/]"
    }
