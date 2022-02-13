module TubeDl.Regex

open System.Text.RegularExpressions

let replace (expr : string) (toReplaceWith : string) (str : string) =
    Regex.Replace (str, expr, toReplaceWith)
