module TubeDl.Version

open System.Reflection

let parseVersion () =
    let ass = Assembly.GetExecutingAssembly()
    let version = ass.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion
    version
