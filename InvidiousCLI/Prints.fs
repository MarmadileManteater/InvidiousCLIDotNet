module  MarmadileManteater.InvidiousCLI.Prints

open System.Collections.Generic
open System
open System.Reflection
open MarmadileManteater.InvidiousClient.Objects.Data


let printAsColor(content : string, foregroundColor : ConsoleColor, backgroundColor : ConsoleColor) =
    let previousForeground = Console.ForegroundColor
    let previousBackground = Console.BackgroundColor
    Console.ForegroundColor <- foregroundColor
    Console.BackgroundColor <- backgroundColor
    Console.Write(content)
    Console.ForegroundColor <- previousForeground
    Console.BackgroundColor <- previousBackground

let printAsColorNewLine(content : string, foregroundColor : ConsoleColor, backgroundColor : ConsoleColor) =
    printAsColor(content, foregroundColor, backgroundColor)
    Console.WriteLine()

let printDictionaryWithColor(dictionary : IDictionary<string, KeyValuePair<string, List<ConsoleColor>>>) =
    Console.WriteLine()
    let mutable maxLength = 0
    for key in dictionary.Keys do
        maxLength <- if key.Length + 1 > maxLength then key.Length + 1 else maxLength
    done
    for keyValuePair in dictionary do
        let key = keyValuePair.Key
        let mutable value = keyValuePair.Value.Key
        let colorInformation = keyValuePair.Value.Value
        printAsColor(key + ":", colorInformation[0], Console.BackgroundColor)
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
        printAsColorNewLine(value, colorInformation[1], Console.BackgroundColor)
    done

let printDictionary(dictionary : IDictionary<string, string>) =
    let psuedoDict = new Dictionary<string, KeyValuePair<string, List<ConsoleColor>>>()
    for keyValuePair in dictionary do
        let psuedoPair = new KeyValuePair<string, List<ConsoleColor>> (keyValuePair.Value, new List<ConsoleColor>())
        psuedoPair.Value.Add(ConsoleColor.Gray)
        psuedoPair.Value.Add(ConsoleColor.White)
        psuedoDict[keyValuePair.Key] <- psuedoPair
    done
    printDictionaryWithColor(psuedoDict)

let printShortVideoInfo (video: InvidiousChannelVideo) =
        let printObject : Dictionary<string, string> = new Dictionary<string, string>()
        printObject["Title"] <- video.Title
        printObject["Author"] <- video.Author
        printObject["VideoId"] <- video.VideoId
        printDictionary(printObject)

let printChannelInfo (channel: InvidiousChannel) =
        let printObject : Dictionary<string, string> = new Dictionary<string, string>()
        printObject["Channel"] <- channel.Author
        printObject["ChannelId"] <- channel.AuthorId
        printDictionary(printObject)

let printPlaylistInfo (playlist : InvidiousPlaylist) =
        let printObject : Dictionary<string, string> = new Dictionary<string, string>()
        printObject["Playlist"] <- playlist.Title
        printObject["Author"] <- playlist.Author
        printObject["PlaylistId"] <- playlist.PlaylistId
        printDictionary(printObject)

let printGreeting () =
    printAsColor("Welcome to the ", ConsoleColor.Gray, Console.BackgroundColor)
    printAsColor("Invidious CLI Client v", ConsoleColor.White, Console.BackgroundColor)
    printAsColor(Assembly.GetEntryAssembly().GetName().Version.ToString(), ConsoleColor.Blue, ConsoleColor.Black)
    printAsColorNewLine("!", ConsoleColor.Gray, Console.BackgroundColor)