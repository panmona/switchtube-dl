module TubeDl.File

open System.IO

let create = File.Create

let exists = File.Exists

let tryFindFileByRegex path regex =
    let files = Directory.EnumerateFiles path
    files |> Seq.tryFind (Regex.matches regex)
