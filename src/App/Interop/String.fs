module TubeDl.String

open System.Text

let toCharArray (str : string) = str.ToCharArray ()

let normalize (str : string) = str.Normalize NormalizationForm.FormD

let replace (old : string) (new' : string) (str : string) = str.Replace (old, new')
