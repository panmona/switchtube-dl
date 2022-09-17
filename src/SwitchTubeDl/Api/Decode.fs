[<RequireQualifiedAccess>]
module TubeDl.Decode

open Thoth.Json.Net

let channelDetails = Decode.fromString ChannelDetails.decoder

let channelVideos = Decode.fromString (Decode.list VideoDetailsApi.decoder)

let videoDetails = Decode.fromString VideoDetailsApi.decoder

let videoPaths = Decode.fromString (Decode.list VideoPath.decoder)
