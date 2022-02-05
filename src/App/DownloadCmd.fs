module TubeDl.DownloadCmd

open TubeDl.Cli


let runDownload d =
    let inValidArgs =
        [
            match DownloadArgParse.tryGetDownloadType d with
            | Some _ -> ()
            | None ->
                // TODO use Spectre.Console for proper formatting
                printfn "- Specify one of the different download types with --video, --channel or --url"
                true

            match DownloadArgParse.tryGetPath d with
            | Some _ -> ()
            | None ->
                // TODO use Spectre.Console for proper formatting
                printfn "- The configured path should be absolute"
                true

                (*match DownloadArgParse.tryGetToken d with
                | Some _ -> ()
                | None ->
                    printfn $"- Specify a token with --token or set the env variable $%s{Constants.tokenEnvName}"
                    true*)
        ]

    if List.containsElements inValidArgs then
        Error (ArgumentsNotSpecified "")
    else

    let downloadCfg = DownloadArgParse.initCfgFromArgs d
    printfn "%A" downloadCfg
    // TODO further implement it
    Ok ()
