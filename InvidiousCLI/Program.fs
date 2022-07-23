module MarmadileManteater.InvidiousCLI.Program

open System.Collections.Generic
open System
open System.Linq
open System.Diagnostics
open System.IO
open Newtonsoft.Json.Linq
open MarmadileManteater.InvidiousClient.Objects
open MarmadileManteater.InvidiousClient.Interfaces
open MarmadileManteater.InvidiousClient.Extensions
open MarmadileManteater.InvidiousClient.Objects.Data
open MarmadileManteater.InvidiousCLI.Objects
open MarmadileManteater.InvidiousCLI.Prints
open MarmadileManteater.InvidiousCLI.FileOperations
open MarmadileManteater.InvidiousCLI.Paths

// This is the central entry point for all of the command processing.
// It is recursive to handle commands that need to be parsed before processing.
// If a user enters a link, it parses out the id for the video and runs
// processCommand(["watch", "{videoId}"], client, userData, false) where {videoId} is id pulled out of the link.
let rec processCommand (args : IList<string>, client : IInvidiousAPIClient, userData : UserData, takeAdditionalInput : bool) =
    // Saving to command history
    userData.AddToCommandHistory(String.Join(" ", args)) |> ignore
    saveUserData(userData)
    //
    if args.Count > 0 then
        if args[0] = "search" then
            if args.Count < 2 then
                // If not enough arguments are given,
                printAsColorNewLine("search requires at least 1 argument", ConsoleColor.Red, Console.BackgroundColor)
                Console.WriteLine()
                printAsColorNewLine("Usage:", ConsoleColor.DarkYellow, Console.BackgroundColor)
                printAsColorNewLine("@param query : string", ConsoleColor.DarkYellow, Console.BackgroundColor)
                printAsColorNewLine("search {query}", ConsoleColor.Yellow, Console.BackgroundColor)
                Console.WriteLine()
            else
                // If there are enough arguments,
                let mutable query = ""
                let mutable i = 0
                for arg in args do
                    if i <> 0 then
                        query <- query + arg + " "
                    i <- i + 1
                done
                // All of the arguments are the query.
                let results = client.SearchSync(query)
                for result in results do
                    if result.IsVideo() then
                        let video = result.ToVideo()
                        printShortVideoInfo(video)
                    elif result.IsChannel() then
                        let channel = result.ToChannel()
                        printChannelInfo(channel)
                    elif result.IsPlaylist() then
                        let playlist = result.ToPlaylist()
                        printPlaylistInfo(playlist)
                done
                let mutable page = 0
                if takeAdditionalInput then
                    while true do
                        let input = System.Console.ReadLine()
                        let innerArguments = input.Split(" ")
                        if innerArguments.Length > 0 then
                            let command = innerArguments[0]

                            if command = "next" then
                                page <- page + 1
                                let results = client.SearchSync(query, page)
                                for result in results do
                                    if result.IsVideo() then
                                        let video = result.ToVideo()
                                        printShortVideoInfo(video)
                                    elif result.IsChannel() then
                                        let channel = result.ToChannel()
                                        printChannelInfo(channel)
                                    elif result.IsPlaylist() then
                                        let playlist = result.ToPlaylist()
                                        printPlaylistInfo(playlist)
                                done
                            elif command = "previous" then
                                page <- page - 1
                                if page > -1 then
                                    let results = client.SearchSync(query, page)
                                    for result in results do
                                        if result.IsVideo() then
                                            let video = result.ToVideo()
                                            printShortVideoInfo(video)
                                        elif result.IsChannel() then
                                            let channel = result.ToChannel()
                                            printChannelInfo(channel)
                                        elif result.IsPlaylist() then
                                            let playlist = result.ToPlaylist()
                                            printPlaylistInfo(playlist)
                                    done
                                else
                                    page <- 0
                                    printAsColorNewLine("You are already on the first page. There is no previous page.", ConsoleColor.DarkYellow, Console.BackgroundColor)
                            else
                                processCommand(input.Split(" "), client, userData, takeAdditionalInput) |> ignore
                    done
        elif args[0] = "watch" then
            if args.Count < 2 then
                // If not enough arguments are given,
                printAsColorNewLine("watch requires at least 1 argument", ConsoleColor.Red, Console.BackgroundColor)
                Console.WriteLine()
                printAsColorNewLine("Usage:", ConsoleColor.DarkYellow, Console.BackgroundColor)
                printAsColorNewLine("@param videoId : string", ConsoleColor.DarkYellow, Console.BackgroundColor)
                printAsColorNewLine("@param qualityOrItag : string (optional)", ConsoleColor.DarkYellow, Console.BackgroundColor)
                printAsColorNewLine("watch {videoId} {qualityOrItag}", ConsoleColor.Yellow, Console.BackgroundColor)
                Console.WriteLine()
            else
                // If there are enough arguments
                // the first argument is the video id
                let videoId = args[1]
                // the second argument is the quality or
                let quality = if args.Count > 2 then args[2] else "360p"
                // if the second argument doesn't contain the letter 'p', it is interpreted as an itag
                let itag = if quality.Contains('p') then null else quality
                let fields = new List<string>()
                // All the fields useful for history
                fields.Add("videoId")
                fields.Add("formatStreams")
                fields.Add("adaptiveFormats")
                fields.Add("title")
                fields.Add("lengthSeconds")
                fields.Add("author")
                fields.Add("authorId")
                fields.Add("videoThumbnails")
                try
                    let videoObject = client.FetchVideoByIdSync(videoId, fields.ToArray())
                    let formatStreams : IList<FormatStream> = videoObject.FormatStreams
                    let mediumQualityStreams = formatStreams.Where(fun stream -> ((itag = null && stream.Resolution = quality) || stream.Itag = itag))
                    // this is a list of audio streams
                    let secondaryStreams = formatStreams.Where(fun secondary -> (itag = null && secondary.Type.Contains("audio") && secondary.Type.Contains(",") <> true)).ToList()
                    if mediumQualityStreams.Count() > 0 then
                        // start up the first media player listed in the user data
                        let processStartInfo = if userData.MediaPlayers.Count > 0 then new ProcessStartInfo(userData.GetPrimaryMediaPlayer().Value<string>("executable_path")) else new ProcessStartInfo(mediumQualityStreams.FirstOrDefault().Url)
                        // use a combination of both the audio and video streams for the arguments
                        // currently, this method only really works for VLC
                        processStartInfo.Arguments <- if userData.MediaPlayers.Count > 0 then mediumQualityStreams.FirstOrDefault().Url + " --input-slave=" + secondaryStreams.FirstOrDefault().Url else ""
                        processStartInfo.UseShellExecute <- true
                        processStartInfo.WorkingDirectory <- if userData.MediaPlayers.Count > 0 then userData.GetPrimaryMediaPlayer().Value<string>("working_directory") else processStartInfo.WorkingDirectory
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
                        saveUserData(userData)
                        Process.Start(processStartInfo) |> ignore
                    else
                        Console.WriteLine()
                        printAsColorNewLine("A format stream mataching this video quality can not be found.", ConsoleColor.Red, Console.BackgroundColor) |> ignore
                with 
                   | :? AggregateException as ex -> if ex.InnerException.Message.Contains("404 (Not Found)") = true then Console.WriteLine(); printAsColorNewLine("The response code indicates the content was not found.", ConsoleColor.Red, Console.BackgroundColor)
        elif args[0].StartsWith("https://") then
            let uri = new Uri(args[0])
            if uri.AbsolutePath = uri.PathAndQuery then
                // no query
                // probably a string that looks like https://yout.be/videoId
                // the last segment of the url is the videoId
                let arguments = new List<string>()
                arguments.Add("watch")
                arguments.Add(uri.Segments.Last())
                processCommand(arguments, client, userData, takeAdditionalInput)
            else
                // query
                // some part of the query is our video id
                if uri.Query.Contains("v=") then
                    let mutable endOfQuery = uri.Query.Split("v=")[1]
                    if endOfQuery.Contains("&") then
                        endOfQuery <- endOfQuery.Split("&")[0]
                    let arguments = new List<string>()
                    arguments.Add("watch")
                    arguments.Add(endOfQuery)
                    processCommand(arguments, client, userData, takeAdditionalInput)
        elif args[0] = "add-media-player" then
            // If the user wants to setup a media player,
            if args.Count < 2 then
                // If not enough arguments are given,
                printAsColorNewLine("add-media-player requires at least 1 argument", ConsoleColor.Red, Console.BackgroundColor)
                Console.WriteLine()
                printAsColorNewLine("Usage:", ConsoleColor.DarkYellow, Console.BackgroundColor)
                printAsColorNewLine("@param executablePath : string", ConsoleColor.DarkYellow, Console.BackgroundColor)
                printAsColorNewLine("add-media-player {executablePath}", ConsoleColor.Yellow, Console.BackgroundColor)
                Console.WriteLine()
            else
                try
                    let mutable executablePath = ""
                    let mutable i = 0
                    for arg in args do
                        if i <> 0 then
                            executablePath <- executablePath + arg + " "
                        i <- i + 1
                    done
                    let executableUri = new Uri(executablePath)
                    let fileName = executableUri.Segments.LastOrDefault()
                    let appName = if fileName.Contains(".exe") then fileName.Split(".exe")[0] else fileName
                    let path = executablePath.Substring(0, executablePath.Length - fileName.Length - 1)
                    let mediaPlayer = new JObject()
                    mediaPlayer.Add("name", appName)
                    mediaPlayer.Add("executable_path", executablePath)
                    mediaPlayer.Add("working_directory", path)
                    userData.AddMediaPlayer(mediaPlayer)
                    saveUserData(userData)
                    printAsColorNewLine("Media player \"" + appName + "\" added successfully!", ConsoleColor.Green, Console.BackgroundColor)
                with
                    ex -> printAsColorNewLine(ex.Message, ConsoleColor.Red, Console.BackgroundColor) ; printAsColorNewLine("Media player added unsuccessfully.", ConsoleColor.Red, Console.BackgroundColor)
        elif args[0] = "list-media-players" then
            let mutable i = 0
            for mediaPlayer in userData.MediaPlayers do
                let printObject = new Dictionary<string, string>()
                for keyValuePair in mediaPlayer do
                    printObject[keyValuePair.Key] <- keyValuePair.Value.Value<string>()
                done
                printObject["index"] <- i.ToString()
                printDictionary(printObject)
                if userData.GetPrimaryMediaPlayerId() = i then
                    printAsColorNewLine("PRIMARY", ConsoleColor.Green, Console.BackgroundColor)
                Console.WriteLine()
                i <- i + 1
            done
        elif args[0] = "set-primary-media-player" then
            // If the user wants to setup a media player,
            if args.Count < 2 then
                // If not enough arguments are given,
                printAsColorNewLine("set-primary-media-player requires at least 1 argument", ConsoleColor.Red, Console.BackgroundColor)
                Console.WriteLine()
                printAsColorNewLine("Usage:", ConsoleColor.DarkYellow, Console.BackgroundColor)
                printAsColorNewLine("@param mediaPlayerIndex : int", ConsoleColor.DarkYellow, Console.BackgroundColor)
                printAsColorNewLine("set-media-player {mediaPlayerIndex}", ConsoleColor.Yellow, Console.BackgroundColor)
                Console.WriteLine()
            else
                try
                    let mediaPlayerIndex = Int32.Parse(args[1])
                    userData.SetPrimaryMediaPlayer(mediaPlayerIndex) |> ignore
                    let mediaPlayer = userData.GetPrimaryMediaPlayer()
                    let appName = mediaPlayer["name"].Value<string>()
                    printAsColorNewLine("Media player \"" + appName + "\" successfully set as primary!", ConsoleColor.Green, Console.BackgroundColor)
                with
                    ex -> printAsColorNewLine(ex.Message, ConsoleColor.Red, Console.BackgroundColor) ; printAsColorNewLine("Media player added unsuccessfully.", ConsoleColor.Red, Console.BackgroundColor)
        elif args[0] = "show-history" then
            for video in userData.VideoHistory do
                printShortVideoInfo(video)
            done
        else
            let command = args[0]
            printAsColorNewLine("\"" + command + "\" is not recognized as a command.", ConsoleColor.Red, Console.BackgroundColor)
let firstTimeSetup(userData : UserData) =
    printAsColorNewLine("Initializing . . . ", ConsoleColor.Blue, ConsoleColor.Black)
    let checkForExistingPlayer (playerExecutable : string, workingDirectory : string) =
        if File.Exists(playerExecutable) then
            let playerName = playerExecutable.Split(workingDirectory)[1]
            let mediaPlayer = new JObject()
            mediaPlayer.Add("name", playerName)
            mediaPlayer.Add("executable_path", playerExecutable)
            mediaPlayer.Add("working_directory", workingDirectory)
            userData.AddMediaPlayer(mediaPlayer)
            saveUserData(userData)
            printAsColorNewLine(playerName + " automatically added!", ConsoleColor.Green, Console.BackgroundColor)
    // #region Windows
    checkForExistingPlayer("C:\Program Files (x86)\Windows Media Player\wmplayer.exe", "C:\Program Files (x86)\Windows Media Player\\")
    checkForExistingPlayer("C:/Program Files/VideoLAN/VLC/vlc.exe", "C:/Program Files/VideoLAN/VLC/")
    // #endregion
    // #region WSL
    checkForExistingPlayer("/mnt/c/Program Files (x86)/Windows Media Player/wmplayer.exe", "/mnt/c/Program Files (x86)/Windows Media Player/")
    checkForExistingPlayer("/mnt/c/Program Files/VideoLAN/VLC/vlc.exe", "/mnt/c/Program Files/VideoLAN/VLC/")
    // #endregion
    // #region Linux
    checkForExistingPlayer("/usr/bin/vlc", "/usr/bin")
    // #endregion
    let mutable command = new List<string>()
    command.Add("list-media-players")
    processCommand(command, null, userData, true) |> ignore
    let mutable input = ""
    if userData.MediaPlayers.Count > 1 then
        printAsColor("Enter the index of the media player you would like to use:", ConsoleColor.Yellow, Console.BackgroundColor)
        input <- System.Console.ReadLine()
    if input = "" then
        printAsColor("Setup a media player? [", ConsoleColor.DarkYellow, Console.BackgroundColor)
        printAsColor("Y", ConsoleColor.Green, Console.BackgroundColor)
        printAsColor("/", ConsoleColor.DarkYellow, Console.BackgroundColor)
        printAsColor("N", ConsoleColor.Red, Console.BackgroundColor)
        printAsColorNewLine("]", ConsoleColor.DarkYellow, Console.BackgroundColor)
        // Loop for validating that the user is either agreeing or declining
        let mutable setupMediaPlayer = System.Console.ReadLine()
        while setupMediaPlayer <> "Y" && setupMediaPlayer <> "N" do
            printAsColor("Setup a media player? [", ConsoleColor.DarkYellow, Console.BackgroundColor)
            printAsColor("Y", ConsoleColor.Green, Console.BackgroundColor)
            printAsColor("/", ConsoleColor.DarkYellow, Console.BackgroundColor)
            printAsColor("N", ConsoleColor.Red, Console.BackgroundColor)
            printAsColorNewLine("]", ConsoleColor.DarkYellow, Console.BackgroundColor)
            setupMediaPlayer <- System.Console.ReadLine()
        done
        if setupMediaPlayer = "Y" then
            // If the user wants to setup a media player,
            printAsColor("Media Player executable path:", ConsoleColor.DarkYellow, ConsoleColor.Black)
            let executablePath = System.Console.ReadLine()
            try
                let executableUri = new Uri(executablePath)
                let fileName = executableUri.Segments.LastOrDefault()
                let appName = if fileName.Contains(".exe") then fileName.Split(".exe")[0] else fileName
                let path = executablePath.Substring(0, executablePath.Length - fileName.Length)
                let mediaPlayer = new JObject()
                mediaPlayer.Add("name", appName)
                mediaPlayer.Add("executable_path", executablePath)
                mediaPlayer.Add("working_directory", path)
                userData.AddMediaPlayer(mediaPlayer)
                saveUserData(userData)
            with
                | ex ->
                    printAsColorNewLine("There was some issue adding the given media player path.", ConsoleColor.Red, Console.BackgroundColor)
                    printAsColorNewLine(ex.Message, ConsoleColor.Red, Console.BackgroundColor)
    else
        let result = try input |> int |> Nullable<int>
                        with:? FormatException ->
                        new Nullable<int>()
        if result.HasValue then
            if userData.MediaPlayers.Count > result.Value then
                command <- new List<string>()
                command.Add("set-primary-media-player")
                command.Add(input)
                processCommand(command, null, userData, true)
[<EntryPoint>]
let main(args) =
    let client = new InvidiousAPIClient()
    if args.Length = 0 then
        // Running with no arguments
        // interactive prompt
        printGreeting()
        let userData = getExistingUserData(firstTimeSetup)
        while true do
            let input = System.Console.ReadLine()
            processCommand(input.Split(" "), client, userData, true) |> ignore
        done
        0
    else
        // non interactive prompt
        let mutable userData = new UserData()
        // If the user data doesn't exist,
        // Don't run first time setup because it will trigger an interactive prompt
        if File.Exists(UserDataPath) then
            userData <- getExistingUserData(firstTimeSetup)
        processCommand(args, client, userData, false) |> ignore
        0
