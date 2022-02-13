module TubeDl.Download.DownloadChannel

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu
open Spectre.Console

open TubeDl
open TubeDl.Cli
open TubeDl.Rich

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
    let displayEpisode =
        List.exists (fun v -> Option.isSome v.Episode) metadata.Videos

    let columns =
        [
            Table.Column.withAlign "Index" Table.Alignment.Center
            Table.Column.init "Title"
            Table.Column.init "Duration"
            Table.Column.withAlign "Date" Table.Alignment.Center
            if displayEpisode then
                Table.Column.withAlign "Episode" Table.Alignment.Center
        ]

    let videoToRow v =
        [
            v.Title
            (VideoDetails.duration >> TimeSpan.hourMinuteSecond) v
            DateTime.isoString v.PublishedAt
            match v.Episode, displayEpisode with
            | Some ep, true -> $"%3s{ep}"
            | None, true -> "" // Pad the table
            | Some _, false // This case shouldn't happen
            | None, false -> ()
        ]

    let rows =
        metadata.Videos
        |> List.sortBy (fun v -> v.PublishedAt)
        |> List.mapi (fun i v -> [ $"%3i{i + 1}" ] @ videoToRow v)

    Table.table TableBorder.Minimal columns rows

let private getSelectionInputFromPrompt max =
    let validation input =
        let errorAnnouncement = ":collision: [bold red]Uh oh![/]"

        match ParseSelection.isValidAndInRange (1, max) input with
        | ParseSelection.Valid -> Ok ()
        | ParseSelection.InvalidTokens e ->
            let tokens =
                e
                |> List.map (fun e -> $"[italic]%s{e}[/]")
                |> String.concat ", "

            $"%s{errorAnnouncement} [bold]Wasn't able to parse your input.[/]\nThe following tokens were invalid: %s{tokens}"
            |> Error
        | ParseSelection.MaxValue i ->
            Error $"%s{errorAnnouncement} The number %i{i} is higher than the allowed: %i{max}"
        | ParseSelection.MinValue i -> Error $"%s{errorAnnouncement} The number %i{i} is lower than 0"

    TextPrompt.promptWithValidation "Download >" validation

let private downloadVideos cfg logCallback (videos : VideoDetails list) =
    async {
        let handleDownload v =
            asyncResult {
                let prefilledCallback = logCallback v.Title
                return! DownloadVideo.handleDownload prefilledCallback cfg v
            }

        let! allRes =
            videos
            |> List.chunkBySize 3
            |> List.map (List.map handleDownload >> Async.parallelCombine)
            |> Async.sequential

        // Map the results for proper error reporting to the parent
        return
            List.concat allRes
            |> List.fold Folder.firstErrorSingleItems (Ok [])
    }

let runDownloadChannel cfg id =
    asyncResult {
        let! metadata =
            let metadataCallback _ctx =
                taskResult { return! downloadChannelMetadata cfg id }

            Status.startDefault "[yellow]Fetching channel metadata[/]" metadataCallback
            |> AsyncResult.mapError DownloadError.TubeInfoError
            |> AsyncResult.teeError (fun e ->
                Markup.printn $":collision: [red][bold]Failure![/] The error [bold]%A{e}[/] occured[/]"
            )

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
            |> ParseSelection.tryParseSelection
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
            taskResult {
                let showFinishedStep video (step : FinishedDlStep) =
                    match step with
                    | FinishedDlStep.Metadata ->
                        Markup.log $"Received video [yellow bold]metadata[/] of \"[italic]%s{video}[/]\""
                    | FinishedDlStep.Download ->
                        Markup.log $"Received video [yellow bold]file[/] \"[italic]%s{video}[/]\""
                    | FinishedDlStep.FileHandling path ->

                    let fileName = FullPath.last path
                    Markup.log $"[yellow bold]Saved file[/] \"[italic]%s{video}[/]\" as \"[italic]%s{fileName}[/]\""

                return! downloadVideos cfg showFinishedStep videosToDownload
            }

        let! _ =
            Status.start
                Spinner.Known.BouncingBar
                $"[bold blue]Downloading videos of channel [yellow underline]%s{metadata.Name}[/][/]"
                callback

        Markup.printn
            $":popcorn: [bold green]Success![/] The requested videos of channel [yellow bold]%s{metadata.Name}[/] were downloaded to [italic]%s{cfg.Path}[/]"
    }
