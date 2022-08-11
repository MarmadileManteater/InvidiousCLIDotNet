namespace MarmadileManteater.InvidiousCLI.Functions

open System.Collections.Generic
open System.Text.RegularExpressions

module CLI =
    let StringToArgumentList(inputString : string) : string[] =
        let argumentsList = Regex.Split(inputString, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")
        let result = Array.create argumentsList.Length null
        for i in 0..argumentsList.Length - 1 do
            let entry = argumentsList[i]
            if entry.StartsWith("\"") && entry.EndsWith("\"") then
                result[i] <- entry.Substring(1, entry.Length - 2)
            else
                result[i] <- entry
        result