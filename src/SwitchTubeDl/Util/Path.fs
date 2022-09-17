[<RequireQualifiedAccess>]
module TubeDl.Path

open System.IO

let combine path1 path2 = Path.Combine (path1, path2)

let isFullyQualified (path : string) = Path.IsPathFullyQualified path

let directorySeparator = Path.DirectorySeparatorChar
