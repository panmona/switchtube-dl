[<RequireQualifiedAccess>]
module TubeDl.File

open System.IO

let create = File.Create

let exists = File.Exists
