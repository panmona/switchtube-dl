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

// TODO next step: write file handling funcs
// TODO then write actual cli
