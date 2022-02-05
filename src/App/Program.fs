module TubeDl.Program

open System.IO
open Microsoft.FSharpLu

// TODO Remove FsharpX.Extras if I don't need it in the end

let token = ""

let path = TubeInfo.pathForVideo token "tFfhu240GV"

let extension =
    match path with
    | Ok p -> MediaType.extension p.MediaType
    | Error e -> failwith (sprintf "%A" e)

let videoStream =
    path
    |> Result.bind (VideoPath.path >> TubeInfo.downloadVideo token)

match videoStream with
| Ok stream ->
    let fileName =
        FileHandling.fileNameFromTitle "OSI Modell, Topologien und Zugriffsverfahren"

    let fileWithExt = $"%s{fileName}.%s{extension}"

    let res' =
        FileHandling.saveFileFromStream FileHandling.OverwriteFile.KeepAsIs "." fileWithExt stream

    match res' with
    | Ok _ -> printfn "it seems to have worked"
    | Error e -> printfn "%A" e
| Error errorValue -> printfn "%A" errorValue

// TODO next step: write file handling funcs
// TODO then write actual cli
