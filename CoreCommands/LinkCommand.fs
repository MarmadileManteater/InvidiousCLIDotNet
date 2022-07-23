namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousClient.Interfaces
open MarmadileManteater.InvidiousCLI.Objects
open System.Collections.Generic
open System.Linq
open System
open MarmadileManteater.InvidiousCLI.Enums

type LinkCommand() =
    interface ICommand with
        member self.Description: string = 
            "Parses the important info out of a given link and calls an associated core command"
        member self.Documentation: System.Collections.Generic.IEnumerable<string> = 
            let results = new List<string>()
            results.Add("@param link {string} the full link")
            results.Add("{link}")
            results
        member self.Execute(args: IList<string>, userData: UserData, client: IInvidiousAPIClient, interactive : bool, processCommand : Action<IList<string>, IInvidiousAPIClient, UserData, bool>): int = 
            let uri = new Uri(args[0])
            if uri.AbsolutePath = uri.PathAndQuery then
                // no query
                // probably a string that looks like https://yout.be/videoId
                // the last segment of the url is the videoId
                let arguments = new List<string>()
                arguments.Add("watch")
                arguments.Add(uri.Segments.Last())
                processCommand.Invoke(arguments, client, userData, interactive)
                0
            else
                // query
                // some part of the query is our video id
                if uri.Query.Contains("v=") then
                    let mutable endOfQuery = uri.Query.Split("v=")[1]
                    if endOfQuery.Contains("&") then
                        endOfQuery <- endOfQuery.Split("&")[0]
                    let arguments = new List<string>()
                    arguments.Add("watch")
                    arguments.Add(endOfQuery)
                    processCommand.Invoke(arguments, client, userData, interactive)
                    0
                else
                    -1
        member self.Match: MatchType = 
            MatchType.StartsWith
        member self.Name: string = 
            "https://"
        member this.RequiredArgCount: int = 
            1