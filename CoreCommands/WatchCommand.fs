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

type WatchCommand() =
    interface ICommand with
        member self.Description: string = 
            "Opens a video in the primary media player"
        member self.Documentation: System.Collections.Generic.IEnumerable<string> = 
            let results = new List<string>()
            results.Add("@param videoId : string")
            results.Add("@param qualityOrItag : string (optional)")
            results.Add("watch {videoId} {qualityOrItag}")
            results
        member self.Execute(args: IList<string>, userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<IList<string>,IInvidiousAPIClient,UserData,bool>): int =  
            let isVideoHistoryEnabled = userData.Settings.IsWatchHistoryEnabled()
            let videoId = args[0]
            // the second argument is the quality or
            let quality = if args.Count > 1 then args[1] else userData.Settings.DefaultFormat()
            // if the second argument doesn't contain the letter 'p', it is interpreted as an itag
            let itag = if quality.Contains('p') then null else quality
            let fields = new List<string>()
            // All the fields useful for history
            fields.Add("formatStreams")
            fields.Add("adaptiveFormats")
            fields.Add("title")
            fields.Add("captions")
            if isVideoHistoryEnabled then
                fields.Add("videoId")
                fields.Add("lengthSeconds")
                fields.Add("author")
                fields.Add("authorId")
                fields.Add("videoThumbnails")
            try
                let videoObject = client.FetchVideoByIdSync(videoId, fields.ToArray())
                let formatStreams : IList<FormatStream> = videoObject.FormatStreams
                let mutable mediumQualityStreams = formatStreams.Where(fun stream -> ((itag = null && stream.Resolution = quality) || stream.Itag = itag))
                // this is a list of audio streams
                let secondaryStreams = formatStreams.Where(fun secondary -> (itag = null && secondary.Type.Contains("audio") && secondary.Type.Contains(",") <> true)).ToList()
                if mediumQualityStreams.Count() = 0 then
                    Prints.PrintAsColorNewLine("There was no format stream of matching quality found.", ConsoleColor.Yellow, Console.BackgroundColor)
                    Prints.PrintAsColorNewLine("Opening the default format for the video instead", ConsoleColor.DarkYellow, Console.BackgroundColor)
                    mediumQualityStreams <- formatStreams.Where(fun stream -> stream.Type.Contains(","))// just get the first audio video stream
                    mediumQualityStreams <- mediumQualityStreams.Reverse()
                // start up the first media player listed in the user data
                let processStartInfo = if userData.MediaPlayers.Count > 0 then new ProcessStartInfo(userData.GetPrimaryMediaPlayer().Value<string>("executable_path")) else new ProcessStartInfo(mediumQualityStreams.FirstOrDefault().Url)
                // use a combination of both the audio and video streams for the arguments
                // currently, this method only really works for VLC
                // i have an idea for how to make this better, but it will involve adding another member to MediaPlayer
                let mutable programSpecificArguments = ""
                if userData.MediaPlayers.Count > 0 then
                    let primaryMediaPlayer = userData.GetPrimaryMediaPlayer()
                    let primaryMediaPlayerName = primaryMediaPlayer["name"].ToString()
                    let mutable inputSlave = if secondaryStreams.Count <> 0 then " --input-slave=" + secondaryStreams.FirstOrDefault().Url else "" 
                    if videoObject.Captions.Count > 0 then
                        let caption = videoObject.Captions.Where(fun x -> x.LanguageCode = "en-US").Last()
                        let httpClient = new HttpClient()
                        let response = httpClient.Send(new HttpRequestMessage(HttpMethod.Get, caption.Url))
                        let videoDirectory = Path.Join(Paths.Temp,videoId)
                        let captionsPath = Path.Join(videoDirectory, caption.Label + ".srt")
                        if Directory.Exists(videoDirectory) <> true then
                            Directory.CreateDirectory(videoDirectory) |> ignore
                        let task = response.Content.ReadAsStringAsync()
                        task.Wait()
                        File.WriteAllText(captionsPath, task.Result)
                        inputSlave <- inputSlave + " --sub-file=\"" + captionsPath + "\""
                    programSpecificArguments <- if primaryMediaPlayerName.Contains("vlc") = true then inputSlave + " --meta-title=\"" + videoObject.Title + "\" --no-one-instance" else ""
                processStartInfo.Arguments <- if userData.MediaPlayers.Count > 0 then mediumQualityStreams.FirstOrDefault().Url + programSpecificArguments else ""
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
                Process.Start(processStartInfo) |> ignore
                0
            with 
                | :? AggregateException as ex -> 
                    if ex.InnerException.Message.Contains("404 (Not Found)") = true then 
                        Console.WriteLine()
                        Prints.PrintAsColorNewLine("The response code indicates the content was not found.", ConsoleColor.Red, Console.BackgroundColor)
                    -1
        member self.Match: Enums.MatchType = 
            Enums.MatchType.Equals
        member self.Name: string = 
            "watch"
        member this.RequiredArgCount: int = 
            1