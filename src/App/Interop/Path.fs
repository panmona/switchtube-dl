module TubeDl.Path

open System.IO

let combine path1 path2 =
    Path.Combine (path1, path2)
