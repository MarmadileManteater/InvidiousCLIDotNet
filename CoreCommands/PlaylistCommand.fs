namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousCLI.Objects
open System.Collections.Generic
open MarmadileManteater.InvidiousCLI
open MarmadileManteater.InvidiousClient.Interfaces
open System

type PlaylistCommand() =
    interface ICommand with
        member self.Description: string = 
            "Displays the contents of a playlist"
        member self.Documentation: IEnumerable<string> = 
            let results = new List<string>()
            results.Add("@param playlistId : string")
            results.Add("#Views the playlist:")
            results.Add("playlist {playlistId}")
            results.Add("#Plays the playlist:")
            results.Add("playlist {playlistId} play")
            results.Add("#Downloads the playlist:")
            results.Add("playlist {playlistId} download")
            results
        member this.Execute(args: IList<string>, userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<IList<string>,IInvidiousAPIClient,UserData,bool>): int = 
            raise (System.NotImplementedException())
        member this.Match: Enums.MatchType = 
            Enums.MatchType.Equals
        member this.Name: string = 
            "playlist"
        member this.RequiredArgCount: int = 
            1