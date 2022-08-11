namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousCLI
open MarmadileManteater.InvidiousClient.Interfaces
open System.Collections.Generic
open System
open MarmadileManteater.InvidiousCLI.Objects
open MarmadileManteater.InvidiousCLI.Functions
open MarmadileManteater.InvidiousCLI.Extensions

type SettingsCommand() =
    inherit ICommand()
        override self.OnInit(pluginObjects : IList<IPluginObject>): unit = 
            ()
        override self.Description: string = 
            "View or change settings"
        override self.Documentation: IEnumerable<string> = 
            [
                "settings list";
                "@param key : string";
                "@param value : string";
                "settings set {key} {value}";
                "@param key : string";
                "settings remove {key}";
                "#Example settings:";
                "- default_server";
                "  settings set default_server https://invidious.sethforprivacy.com";
                "- cache";
                "  settings set cache enable";
                "- video_history";
                "  settings set video_history enable";
                "- command_history";
                "  settings set command_history enable"
            ]
        override self.Execute(args: string[], userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<string[],IInvidiousAPIClient,UserData,bool>): int = 
            if args[0] = "set" then
                if args.Length < 3 then
                    -2// -2 indicating not enough args
                else
                    let mutable settingValue = ""
                    for i in 2..args.Length - 1 do
                        settingValue <- settingValue + args[i] 
                        if i <> args.Length - 1 then
                            settingValue <- settingValue + " "// add spaces
                    userData.SetSetting(args[1], settingValue)
                    FileOperations.SaveUserData(userData)
                    0
            elif args[0] = "get" then
                if args.Length < 2 then
                    -2// -2 indicating not enough args
                else
                    let settingValue = userData.GetSetting(args[1])
                    if settingValue <> null then
                        let dictionary = new Dictionary<string, KeyValuePair<string, List<ConsoleColor>>>()
                        let consoleColors = new List<ConsoleColor>()
                        consoleColors.Add(ConsoleColor.Cyan)
                        consoleColors.Add(ConsoleColor.White)
                        let keyValuePair = new KeyValuePair<string, List<ConsoleColor>>(settingValue.ToString(), consoleColors)
                        dictionary[args[1]] <- keyValuePair
                        Prints.PrintDictionaryWithColor(dictionary)
                        Console.WriteLine()
                    else
                        Prints.PrintAsColorNewLine("No value set for setting \"" + args[1] + "\".", ConsoleColor.DarkYellow, Console.BackgroundColor)
                    0
            elif args[0] = "list" then
                let dictionary = userData.GetSettingsAsDictionary()
                                         .ToStringDictionary()
                if dictionary.ContainsKey("cache") = false then
                    let value = if userData.Settings.IsCacheEnabled() then "enable" else "disable"
                    dictionary["cache"] <- value

                if dictionary.ContainsKey("command_history") = false then
                    let value = if userData.Settings.IsCommandHistoryEnabled() then "enable" else "disable"
                    dictionary["command_history"] <- value

                if dictionary.ContainsKey("default_format") = false then
                    let value = userData.Settings.DefaultFormat()
                    dictionary["default_format"] <- value

                if dictionary.ContainsKey("default_server") = false then
                    let value = if userData.Settings.DefaultServer() <> null then userData.Settings.DefaultServer() else "null"
                    dictionary["default_server"] <- value

                if dictionary.ContainsKey("video_history") = false then
                    let value = if userData.Settings.IsWatchHistoryEnabled() then "enable" else "disable"
                    dictionary["video_history"] <- value
                
                if dictionary.ContainsKey("subtitles") = false then
                    let value = if userData.Settings.AreSubtitlesEnabled() then "enable" else "disable"
                    dictionary["subtitles"] <- value

                if dictionary.ContainsKey("subtitle_language") = false then
                    let value = userData.Settings.SubtitleLanguage()
                    dictionary["subtitle_language"] <- value

                if dictionary.ContainsKey("download_path") = false then
                    let value = userData.Settings.DownloadPath()
                    dictionary["download_path"] <- value
                
                Prints.PrintDictionaryWithTwoColors(dictionary, ConsoleColor.Cyan, ConsoleColor.White)
                Console.WriteLine()
                0
            elif args[0] = "remove" then
                if args.Length < 2 then
                    -2// -2 indicating not enough args
                else
                    userData.RemoveSetting(args[1])
                    FileOperations.SaveUserData(userData)
                    0
            else
                -1
        override self.Match: Enums.MatchType = 
            Enums.MatchType.Equals
        override self.Name: string = 
            "settings"
        override self.RequiredArgCount: int = 
            1

