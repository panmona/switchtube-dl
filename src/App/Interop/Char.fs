module TubeDl.Char

open System.Globalization

let notCategory category (char: char) =
    category <> CharUnicodeInfo.GetUnicodeCategory char

let nonSpacingMark =
    notCategory UnicodeCategory.NonSpacingMark
