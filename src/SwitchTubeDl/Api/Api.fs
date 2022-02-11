module TubeDl.Api

open FsHttp
open FsHttp.DslCE

type Token = | Token of string

[<RequireQualifiedAccess>]
type RequestType =
    | ChannelDetails
    | ChannelVideos
    | VideoDetails
    | VideoPaths
    | DownloadVideo

type ApiError =
    /// Token is incorrect
    | UnauthorizedAccess
    /// This resource was not found
    | ResourceNotFound
    /// Be gentle with the api and send less requests!
    | TooManyRequests
    /// An unexpected api error occured
    | ApiError

let private baseUrl = "https://tube.switch.ch"
let private apiPrefix = $"%s{baseUrl}/api/v1"
let private tokenHeader (Token token) = $"Token %s{token}"

let private channelDetails token channelId =
    httpAsync {
        GET $"%s{apiPrefix}/browse/channels/%s{channelId}"
        CacheControl "no-cache"
        Authorization (tokenHeader token)
    }

// TODO I misread doc somehow! This endpoint uses pagination so I need to gather that code again. For example channel: 34621167
let private channelVideos token channelId =
    httpAsync {
        GET $"%s{apiPrefix}/browse/channels/%s{channelId}/videos"
        CacheControl "no-cache"
        Authorization (tokenHeader token)
    }

let private videoDetails token videoId =
    httpAsync {
        GET $"%s{apiPrefix}/browse/videos/%s{videoId}"
        CacheControl "no-cache"
        Authorization (tokenHeader token)
    }

let private videoPaths token videoId =
    httpAsync {
        GET $"%s{apiPrefix}/browse/videos/%s{videoId}/video_variants"
        CacheControl "no-cache"
        Authorization (tokenHeader token)
    }

/// The asset path should contain the whole relative path
let private downloadVideo _token relativeAssetPath =
    let uri =
        Uri.initRelative baseUrl relativeAssetPath
        |> Uri.absoluteUri

    httpAsync {
        GET uri
        CacheControl "no-cache"
    }

let private request requestType =
    // This function will break when APIs with different params would be needed but these apis will most certainly suffice.
    match requestType with
    | RequestType.ChannelDetails -> channelDetails
    | RequestType.ChannelVideos -> channelVideos
    | RequestType.VideoDetails -> videoDetails
    | RequestType.VideoPaths -> videoPaths
    | RequestType.DownloadVideo -> downloadVideo

let private handleResult (response : Response) =
    match response.statusCode with
    | System.Net.HttpStatusCode.OK -> Ok response
    | System.Net.HttpStatusCode.Unauthorized
    | System.Net.HttpStatusCode.Forbidden -> Error UnauthorizedAccess
    | System.Net.HttpStatusCode.NotFound -> Error ResourceNotFound
    | System.Net.HttpStatusCode.TooManyRequests -> Error TooManyRequests
    | System.Net.HttpStatusCode.InternalServerError
    | _ -> Error ApiError

let api requestType token param =
    async {
        let! res = request requestType token param
        return handleResult res
    }

let toStream = Response.toStreamAsync

let toText = Response.toTextAsync
