module TubeDl.Decode

open Thoth.Json.Net

let channelDetails = Decode.fromString ChannelDetails.decoder

let channelVideos = Decode.fromString (Decode.list VideoDetails.decoder)

let videoDetails = Decode.fromString VideoDetails.decoder

let videoPaths = Decode.fromString (Decode.list VideoPath.decoder)
