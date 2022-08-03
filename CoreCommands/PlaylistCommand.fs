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
open MarmadileManteater.InvidiousCLI.CoreCommands.PlaylistWriters
open System.IO
open MarmadileManteater.InvidiousCLI.Functions
open MarmadileManteater.InvidiousClient.Extensions

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
                results.Add("@param qualityOrItag : string (optional)")
                results.Add("playlist {playlistId} play {qualityOrItag}")
                results.Add("#Downloads the playlist:")
                results.Add("playlist {playlistId} download {qualityOrItag}")
                results
            override self.Execute(args: IList<string>, userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<IList<string>,IInvidiousAPIClient,UserData,bool>): int = 
                let playlistId = args[0]

                let downloadPath = userData.Settings.DownloadPath()
                let playlistPath = Path.Join(downloadPath, playlistId)

                let command = if args.Count > 1 then args[1] else "view"
                // the second argument is the quality or
                let quality = if args.Count > 2 then args[2] else userData.Settings.DefaultFormat()
                let matchedSavedPlaylists = userData.SavedPlaylists.Where(fun playlist -> playlist.Id = playlistId)
                let playlist = if matchedSavedPlaylists.Count() > 0 then matchedSavedPlaylists.First().GetData().ToPlaylist() else client.FetchPlaylistByIdSync(playlistId)
                let urls = new List<string>()
                if command = "download" then
                    Directory.CreateDirectory(playlistPath) |> ignore
                    for video in playlist.Videos do
                        let videoPath = Path.Join(playlistPath, video.VideoId)
                        Directory.CreateDirectory(videoPath) |> ignore
                        let innerArguments = new List<string>()
                        innerArguments.Add("download")
                        innerArguments.Add(video.VideoId)
                        innerArguments.Add(quality)
                        innerArguments.Add(playlistPath)
                        processCommand.Invoke(innerArguments, client, userData, isInteractive)
                        let files = Directory.EnumerateFiles(videoPath)
                        let suffix = files.First().Split("_").Last()
                        urls.Add(Path.Join(video.VideoId, $"{video.VideoId}_{suffix}"))
                    let primaryMediaPlayerName = userData.GetPrimaryMediaPlayer()["name"]
                    let potentialWriters = _playlistWriters.Where(fun writer -> writer.SupportedPlayers.Contains(primaryMediaPlayerName.ToString()))
                    let writer = if potentialWriters.Count() > 0 then potentialWriters.First() else new M3U() // default to m3u because of how generic it is
                    let result = writer.GenerateFileFromPlaylist(playlist, urls)
                    File.WriteAllText(Path.Join(playlistPath, "playlist." + writer.FileType), result)
                    Prints.PrintAsColorNewLine("Succesfully downloaded playlist to directory:", ConsoleColor.Green, Console.BackgroundColor)
                    Prints.PrintAsColorNewLine(playlistPath, ConsoleColor.Green, Console.BackgroundColor)
                0
            override self.Match: Enums.MatchType = 
                Enums.MatchType.Equals
            override self.Name: string = 
                "playlist"
            override self.RequiredArgCount: int = 
                1