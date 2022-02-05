module TubeDl.Decode

open Thoth.Json.Net

let channelDetails = Decode.fromString ChannelDetails.decoder

let channelVideos = Decode.fromString (Decode.list ChannelVideo.decoder)

let videoDetails = Decode.fromString VideoDetails.decoder

// TODO According to the api documentation: "All these endpoints return either a list or a single asset object." Check if this is true.
let videoPaths = Decode.fromString (Decode.list VideoPath.decoder)
