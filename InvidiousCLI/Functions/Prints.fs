namespace MarmadileManteater.InvidiousCLI.Functions

open System.Collections.Generic
open System
open System.Reflection
open MarmadileManteater.InvidiousClient.Objects.Data
open MarmadileManteater.InvidiousCLI.Interfaces

module Prints =
    let PrintAsColor(content : string, foregroundColor : ConsoleColor, backgroundColor : ConsoleColor) =
        let previousForeground = Console.ForegroundColor
        let previousBackground = Console.BackgroundColor
        Console.ForegroundColor <- foregroundColor
        Console.BackgroundColor <- backgroundColor
        Console.Write(content)
        Console.ForegroundColor <- previousForeground
        Console.BackgroundColor <- previousBackground

    let PrintAsColorNewLine(content : string, foregroundColor : ConsoleColor, backgroundColor : ConsoleColor) =
        PrintAsColor(content, foregroundColor, backgroundColor)
        Console.WriteLine()

    let PrintDictionaryWithColor(dictionary : IDictionary<string, KeyValuePair<string, List<ConsoleColor>>>) =
        Console.WriteLine()
        let mutable maxLength = 0
        for key in dictionary.Keys do
            maxLength <- if key.Length + 1 > maxLength then key.Length + 1 else maxLength
        done
        for keyValuePair in dictionary do
            let key = keyValuePair.Key
            let mutable value = keyValuePair.Value.Key
            let colorInformation = keyValuePair.Value.Value
            PrintAsColor(key + ":", colorInformation[0], Console.BackgroundColor)
            for i in 0 .. maxLength - key.Length do
                Console.Write(" ")
            done
            value <- if value.Contains("\n") then 
                            let firstLine : string = value.Split("\n")[0]
                            if firstLine.Length > 255 then
                                firstLine.Substring(0, 255)
                            else
                                firstLine
                        else 
                            value
            PrintAsColorNewLine(value, colorInformation[1], Console.BackgroundColor)
        done

    let PrintDictionary(dictionary : IDictionary<string, string>) =
        let psuedoDict = new Dictionary<string, KeyValuePair<string, List<ConsoleColor>>>()
        for keyValuePair in dictionary do
            let psuedoPair = new KeyValuePair<string, List<ConsoleColor>> (keyValuePair.Value, new List<ConsoleColor>())
            psuedoPair.Value.Add(ConsoleColor.Gray)
            psuedoPair.Value.Add(ConsoleColor.White)
            psuedoDict[keyValuePair.Key] <- psuedoPair
        done
        PrintDictionaryWithColor(psuedoDict)

    let PrintShortVideoInfo (video: InvidiousChannelVideo) =
            let printObject : Dictionary<string, string> = new Dictionary<string, string>()
            printObject["Title"] <- video.Title
            printObject["Author"] <- video.Author
            printObject["VideoId"] <- video.VideoId
            PrintDictionary(printObject)

    let PrintChannelInfo (channel: InvidiousChannel) =
            let printObject : Dictionary<string, string> = new Dictionary<string, string>()
            printObject["Channel"] <- channel.Author
            printObject["ChannelId"] <- channel.AuthorId
            PrintDictionary(printObject)

    let PrintPlaylistInfo (playlist : InvidiousPlaylist) =
            let printObject : Dictionary<string, string> = new Dictionary<string, string>()
            printObject["Playlist"] <- playlist.Title
            printObject["Author"] <- playlist.Author
            printObject["PlaylistId"] <- playlist.PlaylistId
            PrintDictionary(printObject)

    let PrintGreeting () =
        PrintAsColor("Welcome to the ", ConsoleColor.Gray, Console.BackgroundColor)
        PrintAsColor("Invidious CLI Client v", ConsoleColor.White, Console.BackgroundColor)
        PrintAsColor(Assembly.GetEntryAssembly().GetName().Version.ToString(), ConsoleColor.Blue, ConsoleColor.Black)
        PrintAsColorNewLine("!", ConsoleColor.Gray, Console.BackgroundColor)

    let PrintCommandInfo (command : ICommand, prefix : string) =
        PrintAsColorNewLine(prefix + "Description:", ConsoleColor.Yellow, Console.BackgroundColor)
        PrintAsColorNewLine(prefix + command.Description, ConsoleColor.DarkYellow, Console.BackgroundColor)
        PrintAsColorNewLine(prefix + "Usage:", ConsoleColor.Yellow, Console.BackgroundColor)
        for line in command.Documentation do
            if line.StartsWith("@") then
                PrintAsColorNewLine(prefix + line, ConsoleColor.DarkYellow, Console.BackgroundColor)
            elif line.StartsWith("#") then
                PrintAsColorNewLine(prefix + line.Substring(1, line.Length - 1), ConsoleColor.Yellow, Console.BackgroundColor)
            elif line.StartsWith("-") then
                PrintAsColorNewLine(prefix + line, ConsoleColor.Gray, Console.BackgroundColor)
            else
                PrintAsColorNewLine(prefix + line, ConsoleColor.Cyan, Console.BackgroundColor)
        Console.WriteLine()