module TubeDl.Directory

open System.IO

let tryFindFileByRegex path regex =
    let files = Directory.EnumerateFiles path
    files |> Seq.tryFind (Regex.matches regex)
