namespace MarmadileManteater.InvidiousCLI.Extensions

open System.Runtime.CompilerServices
open System.Collections.Generic
open Newtonsoft.Json.Linq


[<Extension>]
type DictionaryExtensions =
    [<Extension>]
    static member ToStringDictionary (jtokenDictionary: IDictionary<string, JToken>) : IDictionary<string, string> =
        let stringDictionary = new Dictionary<string, string>()
        for keyValuePair in jtokenDictionary do
            stringDictionary[keyValuePair.Key] <- keyValuePair.Value.ToString()
        stringDictionary