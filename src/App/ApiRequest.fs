module TubeDl.ApiRequests

open FsHttp.DslCE

[<RequireQualifiedAccess>]
type RequestType =
    | ChannelDetails
    | ChannelVideos
    | VideoDetails
    | VideoPaths

let private baseUrl = "https://tube.switch.ch/api/v1"
let private tokenHeader token = $"Token %s{token}"

let private channelDetails token channelId =
    http {
        GET $"%s{baseUrl}/browse/channels/%s{channelId}"
        CacheControl "no-cache"
        Authorization (tokenHeader token)
    }

let private channelVideos token channelId =
    http {
        GET $"%s{baseUrl}/browse/channels/%s{channelId}/videos"
        CacheControl "no-cache"
        Authorization (tokenHeader token)
    }

let private videoDetails token videoId =
    http {
        GET $"%s{baseUrl}/browse/videos/%s{videoId}"
        CacheControl "no-cache"
        Authorization (tokenHeader token)
    }

let private videoPaths token videoId =
    http {
        GET $"%s{baseUrl}/browse/videos/%s{videoId}/video_variants"
        CacheControl "no-cache"
        Authorization (tokenHeader token)
    }

let api requestType =
    // This function will break when APIs with different params would be needed but these apis will most certainly suffice.
    match requestType with
    | RequestType.ChannelDetails -> channelDetails
    | RequestType.ChannelVideos -> channelVideos
    | RequestType.VideoDetails -> videoDetails
    | RequestType.VideoPaths -> videoPaths
