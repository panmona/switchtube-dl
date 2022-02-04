module TubeDl.Program

open Thoth.Json.Net

let request =
    let api = Api.api Api.RequestType.ChannelVideos
    api "token" "bqCG9XLPHN"
    // Stream.saveFileAsync "a_video.mp4" y |> Async.RunSynchronously

match request with
| Ok response ->
    let t = Api.toText response
    let x = Decode.fromString (Decode.list ChannelVideo.decoder) t
    printfn "%A" x
| Error errorValue -> printfn "%A" errorValue
