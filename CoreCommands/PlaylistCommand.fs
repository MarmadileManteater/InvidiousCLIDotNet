namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousCLI.Objects
open MarmadileManteater.InvidiousCLI.Extensions
open System.Collections.Generic
open MarmadileManteater.InvidiousCLI
open MarmadileManteater.InvidiousClient.Interfaces
open System
open System.Linq
open Newtonsoft.Json.Linq

type PlaylistCommand() =
    inherit ICommand()
        let _playlistWriters : IList<IPlaylistWriter> = new List<IPlaylistWriter>()
            override self.OnInit(pluginObjects : IList<IPluginObject>): unit = 
                let playlistWriters = pluginObjects.Where(fun object -> object.IsPlaylistWriter())
                for writer in playlistWriters do
                    _playlistWriters.Add(writer :?> IPlaylistWriter)
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