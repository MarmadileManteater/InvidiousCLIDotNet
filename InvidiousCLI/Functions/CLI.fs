namespace MarmadileManteater.InvidiousCLI.Functions

open System.Collections.Generic
open System.Text.RegularExpressions

module CLI =
    let StringToArgumentList(inputString : string) : IList<string> =
        let argumentsList = Regex.Split(inputString, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")
        let result = new List<string>()
        for entry in argumentsList do
            if entry.StartsWith("\"") && entry.EndsWith("\"") then
                result.Add(entry.Substring(1, entry.Length - 2))
            else
                result.Add(entry)
        result