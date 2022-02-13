namespace TubeDl

open Microsoft.FSharpLu
open FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
type TubeInfoError =
    | ApiError of Api.ApiError
    | DecodeError of string

module TubeInfo =
    let private mapApiError res =
        AsyncResult.mapError TubeInfoError.ApiError res

    let videoInfo token videoId =
        asyncResult {
            let! response =
                Api.api Api.RequestType.VideoDetails token videoId
                |> mapApiError

            let! txt = Api.toText response

            return!
                Decode.videoDetails txt
                |> Result.mapError TubeInfoError.DecodeError
        }

    let pathForVideo token videoId =
        asyncResult {
            let! response =
                Api.api Api.RequestType.VideoPaths token videoId
                |> mapApiError

            let! txt = Api.toText response

            return!
                Decode.videoPaths txt
                |> Result.map List.head
                |> Result.mapError TubeInfoError.DecodeError
        }

    let channelInfo token channelId =
        asyncResult {
            let! response =
                Api.api Api.RequestType.ChannelDetails token channelId
                |> mapApiError

            let stripHtml inputStr =
                // Be careful before copying this! This pattern won't work for all cases! (https://stackoverflow.com/a/4878506)
                let pattern = "<.*?>"
                System.Text.RegularExpressions.Regex.Replace (inputStr, pattern, "")

            let! txt = Api.toText response

            return!
                Decode.channelDetails txt
                |> Result.map (fun c -> c.Name, stripHtml c.Description)
                |> Result.mapError TubeInfoError.DecodeError
        }

    let channelVideos token channelId =
        asyncResult {
            let! response =
                Api.allChannelVideos token channelId
                |> mapApiError

            let! txts =
                response
                |> List.map Api.toText
                |> Async.sequential

            let decoder =
                Decode.channelVideos
                >> Result.mapError TubeInfoError.DecodeError

            return!
                List.map decoder txts
                |> List.fold Folder.firstErrorList (Ok [])
        }

    let downloadVideo token path =
        asyncResult {
            let! response =
                Api.api Api.RequestType.DownloadVideo token path
                |> mapApiError

            return! Api.toStream response
        }
