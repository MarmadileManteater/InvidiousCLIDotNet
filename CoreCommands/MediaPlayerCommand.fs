namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousClient.Interfaces
open MarmadileManteater.InvidiousCLI.Objects
open System.Collections.Generic
open System.Linq
open Newtonsoft.Json.Linq
open MarmadileManteater.InvidiousCLI.Functions
open System
open MarmadileManteater.InvidiousCLI.Enums
open System.IO
open MarmadileManteater.InvidiousCLI
open MarmadileManteater.InvidiousCLI.Environment

type MediaPlayerCommand() =
    inherit ICommand()
        override self.OnInit(pluginObjects : IList<IPluginObject>): unit = 
            ()
        override self.Description: string =
            "Adds and lists media players available in the file located in \"" + Paths.UserDataPath + "\""
        override self.Documentation: System.Collections.Generic.IEnumerable<string> = 
            let results = new List<string>()
            results.Add("media-player list")
            results.Add("@param executable-uri : string - the location of the uri to be added")
            results.Add("media-player add {executable-uri}")
            results.Add("@param media-player-index : int - the index of the media player to set to primary")
            results.Add("media-player set-primary {media-player-index}")
            results
        override self.Execute(args: IList<string>, userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<IList<string>,IInvidiousAPIClient,UserData,bool>): int = 
            if args[0] = "add" then
                try
                    let mutable executablePath = ""
                    let mutable i = 0
                    for arg in args do
                        if i <> 0 then
                            executablePath <- executablePath + arg + " "
                        i <- i + 1
                    done
                    let executableUri = executablePath.Replace("\\", "/").Split("/")
                    let mutable fileName = executableUri.LastOrDefault()
                    let mutable arguments = ""
                    if fileName.Contains(" ") then
                        let fileNameSplit = fileName.Split(" ")
                        fileName <- fileNameSplit[0]
                        if fileNameSplit.Count() > 1 then
                            arguments <- fileNameSplit[1]
                            for i in 2..fileNameSplit.Count() - 1 do
                                arguments <- arguments + " " + fileNameSplit[i]
                    let appName = if fileName.Contains(".exe") then fileName.Split(".exe")[0] else fileName
                    if arguments.Trim().Length > 0 then
                        executablePath <- executablePath.Replace(arguments.Trim(), "")
                    let path = executablePath.Split(appName)[0]
                    let mediaPlayer = new JObject()
                    mediaPlayer.Add("name", appName)
                    mediaPlayer.Add("executable_path", executablePath)
                    mediaPlayer.Add("working_directory", path)
                    mediaPlayer.Add("arguments", arguments)
                    userData.AddMediaPlayer(mediaPlayer)
                    FileOperations.SaveUserData(userData)
                    Prints.PrintAsColorNewLine("Media player \"" + appName + "\" added successfully!", ConsoleColor.Green, Console.BackgroundColor)
                    0
                with
                    ex -> 
                        Prints.PrintAsColorNewLine(ex.Message, ConsoleColor.Red, Console.BackgroundColor)
                        Prints.PrintAsColorNewLine("Media player added unsuccessfully.", ConsoleColor.Red, Console.BackgroundColor)
                        -1
            elif args[0] = "list" then
                let mutable i = 0
                for mediaPlayer in userData.MediaPlayers do
                    let printObject = new Dictionary<string, string>()
                    for keyValuePair in mediaPlayer do
                        printObject[keyValuePair.Key] <- keyValuePair.Value.Value<string>()
                    done
                    printObject["index"] <- i.ToString()
                    Prints.PrintDictionary(printObject)
                    if userData.GetPrimaryMediaPlayerId() = i then
                        Prints.PrintAsColorNewLine("PRIMARY", ConsoleColor.Green, Console.BackgroundColor)
                    Console.WriteLine()
                    i <- i + 1
                done
                0
            elif args[0] = "set-primary" then
                // If the user wants to setup a media player,
                try
                    let mediaPlayerIndex = Int32.Parse(args[1])
                    userData.SetPrimaryMediaPlayer(mediaPlayerIndex) |> ignore
                    FileOperations.SaveUserData(userData)
                    let mediaPlayer = userData.GetPrimaryMediaPlayer()
                    let appName = mediaPlayer["name"].Value<string>()
                    Prints.PrintAsColorNewLine("Media player \"" + appName + "\" successfully set as primary!", ConsoleColor.Green, Console.BackgroundColor)
                    0
                with
                    ex -> 
                        Prints.PrintAsColorNewLine(ex.Message, ConsoleColor.Red, Console.BackgroundColor)
                        Prints.PrintAsColorNewLine("Media player added unsuccessfully.", ConsoleColor.Red, Console.BackgroundColor) 
                        -1
            else
                -1
        override self.Match: Enums.MatchType = 
            Enums.MatchType.Equals
        override self.Name: string = 
            "media-player"
        override self.RequiredArgCount: int = 
            1