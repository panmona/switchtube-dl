module TubeDl.Decode

open Thoth.Json.Net

let channelDetails =
    Decode.fromString ChannelVideo.decoder

let channelVideos =
    Decode.fromString (Decode.list ChannelVideo.decoder)

let videoDetails =
    Decode.fromString VideoDetails.decoder

let videoPaths =
    Decode.fromString (Decode.list VideoPath.decoder)
