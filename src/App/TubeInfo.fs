module TubeDl.TubeInfo

[<RequireQualifiedAccess>]
type TubeInfoError =
    | ApiError of Api.ApiError
    | DecodeError of string

let videoInfo token videoId =
    let apiRes = Api.api Api.RequestType.VideoDetails token videoId

    match apiRes with
    | Ok response ->
        Api.toText response
        |> Decode.videoDetails
        |> Result.mapError TubeInfoError.DecodeError
    | Error e -> TubeInfoError.ApiError e |> Error

let pathForVideo token videoId =
    let apiRes = Api.api Api.RequestType.VideoPaths token videoId

    match apiRes with
    | Ok response ->
        Api.toText response
        |> Decode.videoPaths
        |> Result.map List.head
        |> Result.mapError TubeInfoError.DecodeError
    | Error e -> TubeInfoError.ApiError e |> Error

let pathsForVideos token videoIds =
    videoIds |> List.map (pathForVideo token)

let channelInfo token channelId =
    let apiRes = Api.api Api.RequestType.ChannelDetails token channelId

    let stripHtml inputStr =
        // Be careful before copying this! This pattern won't work for all cases! (https://stackoverflow.com/a/4878506)
        let pattern = "<.*?>"
        System.Text.RegularExpressions.Regex.Replace (inputStr, pattern, "")

    match apiRes with
    | Ok response ->
        Api.toText response
        |> Decode.channelDetails
        |> Result.map (fun c -> c.Name, stripHtml c.Description)
        |> Result.mapError TubeInfoError.DecodeError
    | Error e -> TubeInfoError.ApiError e |> Error

let channelVideos token channelId =
    let apiRes = Api.api Api.RequestType.ChannelVideos token channelId

    match apiRes with
    | Ok response ->
        Api.toText response
        |> Decode.channelVideos
        |> Result.mapError TubeInfoError.DecodeError
    | Error e -> TubeInfoError.ApiError e |> Error

let downloadVideo token path =
    let apiRes = Api.api Api.RequestType.DownloadVideo token path

    match apiRes with
    | Ok response -> Api.toStream response |> Ok
    | Error e -> TubeInfoError.ApiError e |> Error
