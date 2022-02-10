module TubeDl.DateTime

open System

let isoString (d : DateTimeOffset) = d.ToString ("yyyy-MM-dd")
