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
open Newtonsoft.Json
open System.Numerics
open System.Diagnostics
open System.Threading
open MarmadileManteater.InvidiousClient.Objects.Data
open MarmadileManteater.InvidiousCLI.CoreCommands.Extensions

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
                [
                    "@param playlistId : string";
                    "@param qualityOrItag : string (optional)";
                    "#Views the playlist:";
                    "playlist {playlistId}";
                    "#Plays the playlist:";
                    "playlist {playlistId} play {qualityOrItag}";
                    "#Downloads the playlist:";
                    "playlist {playlistId} download {qualityOrItag}"
                    "@param name : string - the name of the playlist"
                    "@param playlistId : string - the id of the playlist"
                    "@param videoIds : list - all of the trailing arguments are video ids until the last argument which will only be included if it is not \"download\""
                    "playlist new {name} {playlistId} {videoIds}"
                    "playlist new {name} {playlist} {videoIds} download"
                ]
            override self.Execute(args: IList<string>, userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<IList<string>,IInvidiousAPIClient,UserData,bool>): int = 
                let playlistId = args[0]
                let downloadPath = userData.Settings.DownloadPath()
                let playlistPath = Path.Join(downloadPath, playlistId)
                if playlistId = "new" then// if video id is new
                    if args.Count < 4 then
                        -2// not enough arguments
                    else
                        let title = args[1]
                        let givenPlaylistId = args[2]
                        let playlistsCantBeMade = userData.SavedPlaylists.Where(fun playlist -> playlist.Id = givenPlaylistId).Count() > 0 || givenPlaylistId = "new"
                        if playlistsCantBeMade = true then
                            Prints.PrintAsColorNewLine($"Given playlist id \"{givenPlaylistId}\" either already exists or is forbidden.", ConsoleColor.Red, Console.BackgroundColor)
                            -1
                        else
                            let jObject = new JObject()
                            jObject["title"] <- title
                            jObject["playlistId"] <- givenPlaylistId
                            let videoIds = new List<string>()
                            for i in 3..args.Count - 1 do
                                videoIds.Add(args[i])
                                // TODO: need to fetch some basic video data and add it to the JObject
                            0
                else
                    let command = if args.Count > 1 then args[1] else "view"
                    // the second argument is the quality or
                    let quality = if args.Count > 2 then args[2] else userData.Settings.DefaultFormat()
                    let mutable itag = quality
                    let matchedSavedPlaylists = userData.SavedPlaylists.Where(fun playlist -> playlist.Id = playlistId)
                    let playlist = if matchedSavedPlaylists.Count() > 0 then matchedSavedPlaylists.First().GetData().ToPlaylist() else client.FetchPlaylistByIdSync(playlistId)
                    let urls = new List<string>()
                    if command = "play" then
                        let mutable hasPlayed = false
                        let savedPlaylistsMatchingGivenId = userData.SavedPlaylists.Where(fun saved -> saved.Id = playlist.PlaylistId && saved.DownloadFormats.Contains(itag))
                        if savedPlaylistsMatchingGivenId.Count() > 0 then
                            // play from local
                            let playlist = savedPlaylistsMatchingGivenId.First()
                            if playlist.QualityFormats.Keys.Contains(itag) then
                                itag <- playlist.QualityFormats[itag]
                            let playlistPath = Path.Join(userData.Settings.DownloadPath(), playlist.Id)
                            if Directory.Exists(playlistPath) then
                                let playlistFiles = Directory.EnumerateFiles(playlistPath)
                                let name : string = userData.GetPrimaryMediaPlayer().Value<string>("name")
                                let writer = _playlistWriters.Where(fun writer -> writer.SupportedPlayers.Contains(name)).Last()
                                let playlistFilesThatAreTheMatchingItag = playlistFiles.Where(fun file -> file.Contains($"{itag}.{writer.FileType}"))
                                if playlistFilesThatAreTheMatchingItag.Count() > 0 then
                                    let playlistFileName = playlistFilesThatAreTheMatchingItag.First()
                                    let processStartInfo = if userData.MediaPlayers.Count > 0 then new ProcessStartInfo(userData.GetPrimaryMediaPlayer().Value<string>("executable_path").Trim()) else new ProcessStartInfo("")
                                    processStartInfo.Arguments <- playlistFileName
                                    processStartInfo.UseShellExecute <- true
                                    processStartInfo.WorkingDirectory <- if userData.MediaPlayers.Count > 0 then userData.GetPrimaryMediaPlayer().Value<string>("working_directory") else processStartInfo.WorkingDirectory
                                    async {
                                        Process.Start(processStartInfo).WaitForExitAsync() |> Async.AwaitTask |> ignore
                                        hasPlayed <- true
                                    } |> Async.StartAsTask |> ignore

                        if hasPlayed = false then
                            for video in playlist.Videos do
                                // There were no existing playlists found of the selected quality.
                                if itag = quality then
                                    itag <- null
                                let mediumQualityStreams = client.FetchFormatStreams(video.VideoId, quality, itag, userData.Settings.IsWatchHistoryEnabled(), userData.Settings.AreSubtitlesEnabled())[0]
                                if mediumQualityStreams <> null then
                                    urls.Add(mediumQualityStreams.First().Url)
                            let primaryMediaPlayerName = userData.GetPrimaryMediaPlayer()["name"]
                            let potentialWriters = _playlistWriters.Where(fun writer -> writer.SupportedPlayers.Contains(primaryMediaPlayerName.ToString()))
                            let writer = if potentialWriters.Count() > 0 then potentialWriters.Last() else new M3U() // default to m3u because of how generic it is
                            let result = writer.GenerateFileFromPlaylist(playlist, urls)
                            Directory.CreateDirectory(playlistPath) |> ignore
                            File.WriteAllText(Path.Join(playlistPath, $"temp.{writer.FileType}"), result)
                        ()
                    elif command = "download" then
                        Directory.CreateDirectory(playlistPath) |> ignore
                        for video in playlist.Videos do
                            let videoPath = Path.Join(playlistPath, quality, video.VideoId)
                            Directory.CreateDirectory(videoPath) |> ignore
                            processCommand.Invoke(["download"; video.VideoId; quality; Path.Join(playlistPath, quality)].ToList(), client, userData, isInteractive)
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
                            
                                let mutable videoId = ""
                                if videoIdWithSuffix.Contains("_") then
                                    let parts = videoIdWithSuffix.Split("_").ToList()
                                    parts.RemoveAt(parts.Count - 1)
                                    videoId <- parts |> String.concat "_"
                                else
                                    videoId <- videoIdWithSuffix
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
                        // Save the playlist to the saved playlists
                        let existingPlaylists = userData.SavedPlaylists.Where(fun saved -> saved.Id = playlist.PlaylistId)
                        let savedPlaylist = if existingPlaylists.Count() > 0 then existingPlaylists.First() else new SavedPlaylist(playlist.GetData())
                    
                        if existingPlaylists.Count() = 0 then
                            savedPlaylist.AddDownloadFormat(itag)
                            if itag <> quality then
                                savedPlaylist.AddDownloadFormat(quality)
                                savedPlaylist.AddQualityFormat(quality, itag)
                            userData.AddSavedPlaylist(savedPlaylist)
                            FileOperations.SaveUserData(userData)
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
                        if isInteractive then
                            let mutable hasControl = true
                            while hasControl do
                                let input = System.Console.ReadLine()
                                let innerArguments = CLI.StringToArgumentList(input)
                                if innerArguments.Count > 0 then
                                    let command = innerArguments[0]
                                    if ["download"; "view"].Contains(command) then
                                        processCommand.Invoke(CLI.StringToArgumentList($"playlist {playlist.PlaylistId} {input}"), client, userData, isInteractive)
                                    else
                                        processCommand.Invoke(CLI.StringToArgumentList(input), client, userData, isInteractive)
                                        // return control to the main program
                                        hasControl <- false
                    0
            override self.Match: Enums.MatchType = 
                Enums.MatchType.Equals
            override self.Name: string = 
                "playlist"
            override self.RequiredArgCount: int = 
                1