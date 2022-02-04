module TubeDl.FileHandling

open FsHttp.Helper

let saveFileFromStream path stream =
    Stream.saveFileAsync path stream
    |> Async.RunSynchronously
