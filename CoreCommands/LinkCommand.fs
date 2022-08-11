namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousClient.Interfaces
open MarmadileManteater.InvidiousCLI.Objects
open System.Collections.Generic
open System.Linq
open System
open MarmadileManteater.InvidiousCLI.Enums

type LinkCommand() =
    inherit ICommand()
        override self.OnInit(pluginObjects : IList<IPluginObject>): unit = 
            ()
        override self.Description: string = 
            "Parses the important info out of a given link and calls an associated core command"
        override self.Documentation: IEnumerable<string> = 
            [
                "@param link {string} the full link";
                "{link}"
            ]
        override self.Execute(args: string[], userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<string[],IInvidiousAPIClient,UserData,bool>): int = 
            let uri = new Uri(args[0])
            if uri.AbsolutePath = uri.PathAndQuery then
                // no query
                // probably a string that looks like https://yout.be/videoId
                // the last segment of the url is the videoId
                processCommand.Invoke(["watch"; uri.Segments.Last()].ToArray(), client, userData, isInteractive)
                0
            else
                // query
                // some part of the query is our video id
                if uri.Query.Contains("v=") then
                    let mutable endOfQuery = uri.Query.Split("v=")[1]
                    if endOfQuery.Contains("&") then
                        endOfQuery <- endOfQuery.Split("&")[0]
                    processCommand.Invoke(["watch"; endOfQuery].ToArray(), client, userData, isInteractive)
                    0
                else
                    -1
        override self.Match: MatchType = 
            MatchType.StartsWith
        override self.Name: string = 
            "https://"
        override self.RequiredArgCount: int = 
            1