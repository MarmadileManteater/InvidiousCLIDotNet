namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousClient.Interfaces
open MarmadileManteater.InvidiousCLI.Objects
open System.Collections.Generic
open System
open System.Linq
open MarmadileManteater.InvidiousCLI
open MarmadileManteater.InvidiousCLI.Functions
open Newtonsoft.Json.Linq
open System.Net.Http
open System.IO
open MarmadileManteater.InvidiousClient.Objects.Data

type DownloadCommand() =
    inherit ICommand()
        override self.OnInit(pluginObjects : IList<IPluginObject>): unit = 
            ()
        override self.Description: string = 
            "Opens a video in the primary media player"
        override self.Documentation: IEnumerable<string> = 
            [
                "@param videoId : string";
                "@param qualityOrItag : string (optional)";
                "@param path : string (optional)";
                "download {videoId} {qualityOrItag} {path}"
            ]
        override self.Execute(args: string[], userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<string[],IInvidiousAPIClient,UserData,bool>): int = 
            let isVideoHistoryEnabled = userData.Settings.IsWatchHistoryEnabled()
            let areSubtitlesEnabled = userData.Settings.AreSubtitlesEnabled()
            let videoId = args[0]
            let quality = if args.Length > 1 then args[1] else userData.Settings.DefaultFormat()
            // the second argument is the quality or
            let mutable directory = ""
            if args.Length > 1 then
                for i in 1..args.Length - 1 do
                    directory <- args[i]
                    if i <> args.Length - 1 then
                        directory <- " "
            let downloadDirectory = directory
            // if the second argument doesn't contain the letter 'p', it is interpreted as an itag
            let itag = if quality.Contains('p') then null else quality
            let fields = new List<string>()
            // All the fields useful for history
            if isVideoHistoryEnabled then
                fields.Add("title")
                fields.Add("videoId")
                fields.Add("lengthSeconds")
                fields.Add("author")
                fields.Add("authorId")
                fields.Add("videoThumbnails")
            if areSubtitlesEnabled then
                fields.Add("captions")
            try
                if fields.Count > 0 then
                    let videoObject = client.FetchVideoByIdSync(videoId, fields.ToArray())
                    if areSubtitlesEnabled then
                        if videoObject.Captions.Count > 0 then
                            let captions = videoObject.Captions.Where(fun caption -> caption.LanguageCode.Contains(userData.Settings.SubtitleLanguage()))
                            for caption in captions do
                                let httpClient = new HttpClient()
                                let response = httpClient.Send(new HttpRequestMessage(HttpMethod.Get, caption.Url))
                                let videoDirectory = Path.Join(downloadDirectory, videoId)
                                let captionsPath = Path.Join(videoDirectory, videoId + "." + caption.LanguageCode + ".srt")
                                if Directory.Exists(videoDirectory) <> true then
                                    Directory.CreateDirectory(videoDirectory) |> ignore
                                let task = response.Content.ReadAsStringAsync()
                                task.Wait()
                                let content = task.Result
                                if content.Split("\n").Count() > 5 then
                                    File.WriteAllText(captionsPath, content)
                    if isVideoHistoryEnabled then
                        let videoData = videoObject.GetData()
                        let newThumbnails = new JArray()
                        for thumbnail in videoData["videoThumbnails"] do
                            if thumbnail.Value<string>("quality") = "maxresdefault" then
                                newThumbnails.Add(thumbnail)
                        done
                        videoData["videoThumbnails"] <- newThumbnails
                        userData.AddToVideoHistory(videoData) |> ignore
            with
                ex -> Prints.PrintAsColorNewLine(ex.Message, ConsoleColor.Yellow, Console.BackgroundColor)
            let predicate = fun (formatStream: FormatStream) -> 
                (formatStream.QualityLabel = quality && itag = null)  || 
                (quality = itag && formatStream.Itag = itag)
            client.DownloadFirstMatchingVideoFormatSync(videoId, downloadDirectory, predicate)
            0
        override self.Match: Enums.MatchType = 
            Enums.MatchType.Equals
        override self.Name: string = 
            "download"
        override self.RequiredArgCount: int = 
            1