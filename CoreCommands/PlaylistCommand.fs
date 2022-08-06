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
                for object in pluginObjects do
                    if object.IsPlaylistWriter() then
                        _playlistWriters.Add(object :?> IPlaylistWriter)
            override self.Description: string = 
                "Displays the contents of a playlist"
            override self.Documentation: IEnumerable<string> = 
                let results = new List<string>()
                results.Add("@param playlistId : string")
                results.Add("@param qualityOrItag : string (optional)")
                results.Add("#Views the playlist:")
                results.Add("playlist {playlistId}")
                results.Add("#Plays the playlist:")
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
                let mutable itag = quality
                let matchedSavedPlaylists = userData.SavedPlaylists.Where(fun playlist -> playlist.Id = playlistId)
                let playlist = if matchedSavedPlaylists.Count() > 0 then matchedSavedPlaylists.First().GetData().ToPlaylist() else client.FetchPlaylistByIdSync(playlistId)
                let urls = new List<string>()
                if command = "download" then
                    Directory.CreateDirectory(playlistPath) |> ignore
                    for video in playlist.Videos do
                        let videoPath = Path.Join(playlistPath, quality, video.VideoId)
                        Directory.CreateDirectory(videoPath) |> ignore
                        let innerArguments = new List<string>()
                        innerArguments.Add("download")
                        innerArguments.Add(video.VideoId)
                        innerArguments.Add(quality)
                        innerArguments.Add(Path.Join(playlistPath, quality))
                        processCommand.Invoke(innerArguments, client, userData, isInteractive)
                        let files = Directory.EnumerateFiles(videoPath)
                        let mutable srtPath = ""
                        let mutable videoName = ""
                        for file in files do
                            if file.EndsWith(".srt") then
                                srtPath <- file
                            elif file.Contains($"{video.VideoId}/{video.VideoId}_") || file.Contains($"{video.VideoId}\{video.VideoId}_") then
                                videoName <- file
                        let suffix = videoName.Split("_").Last()
                        itag <- suffix.Split(".")[0]
                        let mutable langCode = ""
                        if srtPath <> "" then
                            let parts = srtPath.Split(".").ToList()
                            parts.RemoveAt(parts.Count - 1)
                            langCode <- parts.Last()
                        let parts = videoName.Split("_").Last().Split(".").ToList()
                        parts.RemoveAt(parts.Count - 1)
                        let suffixWithoutFileName = String.Join(".", parts)
                        try
                            File.Move(Path.Join(videoPath, $"{video.VideoId}." + langCode + ".srt"), Path.Join(videoPath, $"{video.VideoId}_{suffixWithoutFileName}." + langCode + ".srt"))
                        with
                            ex -> ()
                        urls.Add(Path.Join(video.VideoId, $"{video.VideoId}_{suffix}"))
                    let primaryMediaPlayerName = userData.GetPrimaryMediaPlayer()["name"]
                    let potentialWriters = _playlistWriters.Where(fun writer -> writer.SupportedPlayers.Contains(primaryMediaPlayerName.ToString()))
                    let writer = if potentialWriters.Count() > 0 then potentialWriters.Last() else new M3U() // default to m3u because of how generic it is
                    let result = writer.GenerateFileFromPlaylist(playlist, urls)
                    let videosInThisQualityDownload = Directory.EnumerateDirectories(Path.Join(playlistPath, quality))
                    for video in videosInThisQualityDownload do
                        let filesInDirectoryPath = Directory.EnumerateFiles(video)
                        for file in filesInDirectoryPath do
                            let fileUriSegments = (new Uri(file)).Segments
                            let fileName = fileUriSegments[fileUriSegments.Length - 1]
                            let videoIdWithSuffix = fileName.Split(".")[0]
                            
                            let videoId = if videoIdWithSuffix.Contains("_") then videoIdWithSuffix.Split("_")[0] else videoIdWithSuffix
                            let finalVideoPath = Path.Join(playlistPath, videoId)
                            try
                                Directory.CreateDirectory(finalVideoPath) |> ignore
                            with
                                ex -> ()
                            try
                                File.Move(file, Path.Join(finalVideoPath, fileName))
                            with
                                ex -> ()
                    Directory.Delete(Path.Join(playlistPath, quality), true)
                    File.WriteAllText(Path.Join(playlistPath, $"{itag}.{writer.FileType}"), result)
                    Prints.PrintAsColorNewLine("Succesfully downloaded playlist to directory:", ConsoleColor.Green, Console.BackgroundColor)
                    Prints.PrintAsColorNewLine(playlistPath, ConsoleColor.Green, Console.BackgroundColor)
                elif command = "view" then
                    // list the playlist
                    let playlistDictionary = new Dictionary<string, string>()
                    playlistDictionary["Title"] <- playlist.Title
                    playlistDictionary["PlaylistId"] <- playlist.PlaylistId
                    playlistDictionary["Author"] <- playlist.Author
                    playlistDictionary["AuthorId"] <- playlist.AuthorId
                    Prints.PrintDictionaryWithTwoColors(playlistDictionary, ConsoleColor.DarkYellow, ConsoleColor.White)
                    for video in playlist.Videos do
                        Prints.PrintShortVideoInfo(video.GetData().ToVideo())
                0
            override self.Match: Enums.MatchType = 
                Enums.MatchType.Equals
            override self.Name: string = 
                "playlist"
            override self.RequiredArgCount: int = 
                1