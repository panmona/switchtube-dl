module TubeDl.DownloadChannel

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu
open Spectre.Console

open TubeDl.Cli
open TubeDl.ParseSelection
open TubeDl.Rich

type ChannelDownloadError =
    | ApiError of TubeInfoError
    | SaveFileError of SaveFileError
    | VideoDownloadError of DownloadVideo.VideoDownloadError

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

let private downloadVideos cfg (videos : VideoDetails list) =
    async {
        let handleDownload v =
            asyncResult {
                return!
                    DownloadVideo.handleDownload (fun _ -> ()) cfg v
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
            |> AsyncResult.mapError ChannelDownloadError.ApiError

        Markup.printn $":sparkles: Found the following videos for channel [bold underline]%s{metadata.Name}[/]:"
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

        let! _ = downloadVideos cfg videosToDownload

        // TODO add callbacks: do just the logs -> Fetched video data for ... , Saved video ...

        return ()
    }
