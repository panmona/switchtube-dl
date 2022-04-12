module TubeDl.Regex

open System.Text.RegularExpressions

let replace (expr : string) (toReplaceWith : string) (str : string) =
    Regex.Replace (str, expr, toReplaceWith)

let escape =
    Regex.Escape

let matches pattern input =
    let m = Regex.Match (input, pattern)
    m.Success
