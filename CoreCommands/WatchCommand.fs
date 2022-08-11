namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousClient.Interfaces
open MarmadileManteater.InvidiousCLI.Objects
open System.Collections.Generic
open System.Linq
open MarmadileManteater.InvidiousClient.Objects.Data
open System.Diagnostics
open Newtonsoft.Json.Linq
open MarmadileManteater.InvidiousCLI.Functions
open System
open MarmadileManteater.InvidiousCLI.Enums
open System.IO
open MarmadileManteater.InvidiousCLI
open System.Net.Http
open MarmadileManteater.InvidiousCLI.CoreCommands.Extensions

type WatchCommand() =
    inherit ICommand()
        override self.OnInit(pluginObjects : IList<IPluginObject>): unit = 
            ()
        override self.Description: string = 
            "Opens a video in the primary media player"
        override self.Documentation: IEnumerable<string> = 
            [
                "@param videoId : string";
                "@param qualityOrItag : string (optional)";
                "watch {videoId} {qualityOrItag}"
            ]
        override self.Execute(args: string[], userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<string[],IInvidiousAPIClient,UserData,bool>): int =  
            let isVideoHistoryEnabled = userData.Settings.IsWatchHistoryEnabled()
            let areSubtitlesEnabled = userData.Settings.AreSubtitlesEnabled()
            let videoId = args[0]
            // the second argument is the quality or
            let quality = if args.Length > 1 then args[1] else userData.Settings.DefaultFormat()
            // if the second argument doesn't contain the letter 'p', it is interpreted as an itag
            let itag = if quality.Contains('p') then null else quality
            try
                let streams = client.FetchFormatStreams(videoId, quality, itag, isVideoHistoryEnabled, areSubtitlesEnabled)
                let mutable secondaryStreams = new List<FormatStream>()
                if streams.Length > 1 then
                    let mediumQualityStreams : IEnumerable<FormatStream> = streams.ElementAt(0)
                    let stream = streams.ElementAt(1)
                    secondaryStreams.Add(stream.First())
                    
                    let videoObject = client.FetchVideoByIdSync(videoId)
                    // start up the first media player listed in the user data
                    let processStartInfo = if userData.MediaPlayers.Count > 0 then new ProcessStartInfo(userData.GetPrimaryMediaPlayer().Value<string>("executable_path").Trim()) else new ProcessStartInfo(mediumQualityStreams.FirstOrDefault().Url)
                    let mutable arguments = ""
                    if userData.MediaPlayers.Count > 0 then
                        let primaryMediaPlayer = userData.GetPrimaryMediaPlayer()
                        arguments <- if primaryMediaPlayer["arguments"] <> null then primaryMediaPlayer["arguments"].Value<string>() else ""
                        let audioStream = if secondaryStreams.Count > 0 then secondaryStreams.Last().Url else mediumQualityStreams.First().Url
                        arguments <- arguments.Replace("{audio_stream}", audioStream)
                        if videoObject.Captions.Count > 0 && areSubtitlesEnabled then
                            let captions = videoObject.Captions.Where(fun x -> x.LanguageCode.Contains(userData.Settings.SubtitleLanguage()))
                            if captions.Count() > 0 then
                                let caption = captions.Last()
                                let httpClient = new HttpClient()
                                let response = httpClient.Send(new HttpRequestMessage(HttpMethod.Get, caption.Url))
                                let videoDirectory = Path.Join(userData.Settings.DownloadPath(),videoId)
                                let captionsPath = Path.Join(videoDirectory, caption.Label + ".srt")
                                if Directory.Exists(videoDirectory) <> true then
                                    Directory.CreateDirectory(videoDirectory) |> ignore
                                let task = response.Content.ReadAsStringAsync()
                                task.Wait()
                                let content = task.Result
                                if content.Split("\n").Count() > 5 then
                                    File.WriteAllText(captionsPath, content)
                                    arguments <- arguments.Replace("{subtitle_file}", captionsPath)
                            else
                                arguments <- arguments.Replace("{subtitle_file}", "")
                        else
                            arguments <- arguments.Replace("{subtitle_file}", "")
                        arguments <- arguments.Replace("{title}", videoObject.Title)
                    processStartInfo.Arguments <- if userData.MediaPlayers.Count > 0 then mediumQualityStreams.FirstOrDefault().Url + " " + arguments else ""
                    processStartInfo.UseShellExecute <- true
                    processStartInfo.WorkingDirectory <- if userData.MediaPlayers.Count > 0 then userData.GetPrimaryMediaPlayer().Value<string>("working_directory") else processStartInfo.WorkingDirectory
                    if isVideoHistoryEnabled then
                        let videoData = videoObject.GetData()
                        videoData["formatStreams"] <- null
                        videoData["adaptiveFormats"] <- null
                        let newThumbnails = new JArray()
                        for thumbnail in videoData["videoThumbnails"] do
                            if thumbnail.Value<string>("quality") = "maxresdefault" then
                                newThumbnails.Add(thumbnail)
                        done
                        videoData["videoThumbnails"] <- newThumbnails
                        userData.AddToVideoHistory(videoData) |> ignore
                    FileOperations.SaveUserData(userData)
                    async {
                        Process.Start(processStartInfo).WaitForExitAsync() |> Async.AwaitTask |> ignore
                    } |> Async.StartAsTask |> ignore
                0
            with 
                | :? AggregateException as ex -> 
                    if ex.InnerException.Message.Contains("404 (Not Found)") = true then 
                        Console.WriteLine()
                        Prints.PrintAsColorNewLine("The response code indicates the content was not found.", ConsoleColor.Red, Console.BackgroundColor)
                    -1
        override self.Match: Enums.MatchType = 
            Enums.MatchType.Equals
        override self.Name: string = 
            "watch"
        override this.RequiredArgCount: int = 
            1