module TubeDl.Program

let request =
    let api = Api.api Api.RequestType.VideoDetails
    api "token" "bqCG9XLPHN"

match request with
| Ok response ->
    let t = Api.toText >> Decode.channelVideos
    t response
    |> printfn "%A"
| Error errorValue ->
    printfn "%A" errorValue
