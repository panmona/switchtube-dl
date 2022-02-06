module TubeDl.DownloadCmd

open Spectre.Console
open TubeDl.Cli

let downloadUrl cfg url = ()

let downloadChannel cfg id = ()

let downloadVideo cfg id =
    AnsiConsole
        .Status()
        .Start (
            $"Downloading video %s{id}",
            fun ctx -> (
                AnsiConsole.MarkupLine("")
                AnsiConsole.MarkupLine($"[grey]LOG:[/] Requesting video info[grey]...[/]");
                System.Threading.Thread.Sleep 1000
                AnsiConsole.MarkupLine($"[grey]LOG:[/] Downloading video[grey]...[/]");
                StatusContextExtensions.Spinner (ctx, Spinner.Known.BouncingBar) |> ignore
                System.Threading.Thread.Sleep 10000
                // TODO success message
            )
        )
    ()

let download cfg =
    match cfg.DownloadType with
    | DownloadType.Video id -> downloadVideo cfg id
    | DownloadType.Channel id -> downloadChannel cfg id
    | DownloadType.Url url -> downloadUrl cfg url

let runDownload res =
    let inValidArgs =
        [
            match DownloadArgParse.tryGetDownloadType res with
            | Some _ -> ()
            | None ->
                // TODO use Spectre.Console for proper formatting
                "- Specify one of the different download types with --video, --channel or --url"

            match DownloadArgParse.tryGetPath res with
            | Some _ -> ()
            | None -> "- The configured path should be absolute"
        ]

    if List.containsElements inValidArgs then
        List.iter (printfn "%s") inValidArgs
        Error (ArgumentsNotSpecified "")
    else

    let cfg = DownloadArgParse.initCfgFromArgs res
    printfn "%A" cfg
    download cfg
    Ok ()
