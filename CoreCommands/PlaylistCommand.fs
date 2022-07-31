namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousCLI.Objects
open System.Collections.Generic
open MarmadileManteater.InvidiousCLI
open MarmadileManteater.InvidiousClient.Interfaces
open System

type PlaylistCommand() =
    inherit ICommand()
        override self.OnInit(pluginObjects : IList<IPluginObject>): unit = 
            ()//
        override self.Description: string = 
            "Displays the contents of a playlist"
        override self.Documentation: IEnumerable<string> = 
            let results = new List<string>()
            results.Add("@param playlistId : string")
            results.Add("#Views the playlist:")
            results.Add("playlist {playlistId}")
            results.Add("#Plays the playlist:")
            results.Add("playlist {playlistId} play")
            results.Add("#Downloads the playlist:")
            results.Add("playlist {playlistId} download")
            results
        override self.Execute(args: IList<string>, userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<IList<string>,IInvidiousAPIClient,UserData,bool>): int = 
            raise (System.NotImplementedException())
        override self.Match: Enums.MatchType = 
            Enums.MatchType.Equals
        override self.Name: string = 
            "playlist"
        override self.RequiredArgCount: int = 
            1