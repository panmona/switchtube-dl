[<RequireQualifiedAccess>]
module TubeDl.Download.DownloadChannel

open FsToolkit.ErrorHandling
open Microsoft.FSharpLu
open Spectre.Console

open TubeDl
open TubeDl.Cli
open TubeDl.Rich

type private ChannelMetadata = {
    Name : string
    Description : string
    Videos : VideoDetails list
}

let private downloadChannelMetadata cfg id = asyncResult {
    let! name, desc = TubeInfo.channelInfo cfg.Token id

    let! videos = TubeInfo.channelVideos cfg.Token id

    return {
        Name = name
        Description = desc
        Videos = videos
    }
}

let private downloadVideos cfg logCallback (videos : VideoDetails list) = async {
    let handleDownload v = asyncResult {
        let prefilledCallback = logCallback v.Title
        return! DownloadVideo.handleDownload cfg prefilledCallback v
    }

    let! allRes =
        videos
        |> List.chunkBySize 3
        |> List.map (List.map handleDownload >> Async.parallelCombine)
        |> Async.sequential

    // Mapping the results for proper error reporting to the parent
    return List.concat allRes |> List.fold Folder.firstErrorSingleItems (Ok [])
}

let private printMetadataTable metadata =
    let displayEpisode =
        List.exists (fun v -> Option.isSome v.EpisodeOpt) metadata.Videos

    let columns = [
        Table.Column.withAlign "Index" Table.Alignment.Center
        Table.Column.init "Title"
        Table.Column.init "Duration"
        Table.Column.withAlign "Date" Table.Alignment.Center
        if displayEpisode then
            Table.Column.withAlign "Episode" Table.Alignment.Center
    ]

    let videoToRow v = [
        v.Title
        (VideoDetails.duration >> TimeSpan.hourMinuteSecond) v
        DateTime.isoString v.PublishedAt
        match v.EpisodeOpt, displayEpisode with
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
            let tokens = e |> List.map (fun e -> $"[italic]%s{e}[/]") |> String.concat ", "

            $"%s{errorAnnouncement} [bold]Wasn't able to parse your input.[/]\nThe following tokens were invalid: %s{tokens}"
            |> Error
        | ParseSelection.MaxValue i ->
            Error $"%s{errorAnnouncement} The number %i{i} is higher than the allowed: %i{max}"
        | ParseSelection.MinValue i -> Error $"%s{errorAnnouncement} The number %i{i} is lower than 0"

    TextPrompt.promptWithValidation "Download >" validation

let private runDownloadFromDetails cfg metadata videoDetails = asyncResult {
    let callback _ctx = taskResult {
        let showFinishedStep videoTitle (step : FinishedDlStep) =
            match step with
            | FinishedDlStep.Metadata ->
                Markup.log $"Received video [yellow bold]metadata[/] of \"[italic]%s{esc videoTitle}[/]\""
            | FinishedDlStep.Download ->
                Markup.log $"Received video [yellow bold]file[/] \"[italic]%s{esc videoTitle}[/]\""
            | FinishedDlStep.FileHandling res ->

            match res with
            | FileWriteResult.Written path ->
                let fileName = FullPath.last path

                $"[green bold]Saved video[/] \"[italic]%s{esc videoTitle}[/]\" as \"[italic]%s{esc fileName}[/]\""
                |> Markup.log
            | FileWriteResult.Skipped path ->
                let fileName = FullPath.last path

                $"[yellow bold]Skipped[/] saving of video \"[italic]%s{esc videoTitle}[/]\" as it already exists as \"[italic]%s{esc fileName}[/]\""
                |> Markup.log

        return! downloadVideos cfg showFinishedStep videoDetails
    }

    let! _ =
        Status.start
            Spinner.Known.BouncingBar
            $"[bold blue]Downloading videos of channel [yellow underline]%s{esc metadata.Name}[/][/]"
            callback

    Markup.printn
        $":popcorn: [bold green]Success![/] The requested videos of channel [yellow bold]%s{esc metadata.Name}[/] were downloaded to [italic]%s{esc cfg.Path}[/]"
}

let private runDownloadInteractive cfg metadata = asyncResult {
    $":sparkles: Found the following videos for channel [yellow bold underline]%s{esc metadata.Name}[/]:"
    |> Markup.printn

    printMetadataTable metadata

    "[bold underline]Choose[/] which videos you want to download by specifying their [yellow bold underline]index[/]\n"
    + "Separate the entries with [yellow bold],[/] and use [yellow bold]-[/] to specify a range ([italic]e.g.: \"1, 3-7, 10\"[/])"
    |> Markup.printn

    let selection = List.length metadata.Videos |> getSelectionInputFromPrompt

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

    return! runDownloadFromDetails cfg metadata videosToDownload
}

let runDownload cfg id = asyncResult {
    let! metadata =
        let metadataCallback _ctx = taskResult { return! downloadChannelMetadata cfg id }

        Status.startDefault "[yellow]Fetching channel metadata[/]" metadataCallback
        |> AsyncResult.mapError DownloadError.TubeInfoError

    match cfg.ChannelFilter with
    | Some ChannelFilter.All -> return! runDownloadFromDetails cfg metadata metadata.Videos
    | None -> return! runDownloadInteractive cfg metadata

}
