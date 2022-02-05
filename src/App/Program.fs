module TubeDl.Program

// TODO Remove FsharpX.Extras if I don't need it in the end

let token = "token"
let res = TubeInfo.channelVideos token "bqCG9XLPHN"

match res with
| Ok videos -> printfn "%A" videos
| Error errorValue -> printfn "%A" errorValue
