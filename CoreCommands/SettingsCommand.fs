namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousCLI
open MarmadileManteater.InvidiousClient.Interfaces
open System.Collections.Generic
open System
open MarmadileManteater.InvidiousCLI.Objects
open MarmadileManteater.InvidiousCLI.Functions

type SettingsCommand() =
    interface ICommand with

        member self.Description: string = 
            "View or change settings"
        member self.Documentation: System.Collections.Generic.IEnumerable<string> = 
            let results = new List<string>()
            results.Add("settings list")
            results.Add("@param key : string")
            results.Add("@param value : string")
            results.Add("settings set {key} {value}")
            results.Add("@param key : string")
            results.Add("settings get {key}")
            results.Add("@param key : string")
            results.Add("settings remove {key}")
            results.Add("#Example settings:")
            results.Add("- default_server")
            results.Add("  settings set default_server https://invidious.sethforprivacy.com")
            results.Add("- cache")
            results.Add("  settings set cache enable")
            results.Add("- video_history")
            results.Add("  settings set video_history enable")
            results.Add("- command_history")
            results.Add("  settings set command_history enable")
            results
        member self.Execute(args: IList<string>, userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<IList<string>,IInvidiousAPIClient,UserData,bool>): int = 
            if args[0] = "set" then
                if args.Count < 3 then
                    -2// -2 indicating not enough args
                else
                    let mutable settingValue = ""
                    for i in 2..args.Count - 1 do
                        settingValue <- settingValue + args[i] 
                        if i <> args.Count - 1 then
                            settingValue <- settingValue + " "// add spaces
                    userData.SetSetting(args[1], settingValue)
                    FileOperations.SaveUserData(userData)
                    0
            elif args[0] = "get" then
                if args.Count < 2 then
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
                let dictionary = new Dictionary<string, KeyValuePair<string, List<ConsoleColor>>>()
                let settingsDictionary = userData.GetSettingsAsDictionary()
                let consoleColors = new List<ConsoleColor>()
                consoleColors.Add(ConsoleColor.Cyan)
                consoleColors.Add(ConsoleColor.White)
                for setting in settingsDictionary do
                    let keyValuePair = new KeyValuePair<string, List<ConsoleColor>>(setting.Value.ToString(), consoleColors)
                    dictionary[setting.Key] <- keyValuePair

                if dictionary.ContainsKey("cache") = false then
                    let value = if userData.Settings.IsCacheEnabled() then "enable" else "disable"
                    dictionary["cache"] <- new KeyValuePair<string, List<ConsoleColor>>(value, consoleColors)

                if dictionary.ContainsKey("command_history") = false then
                    let value = if userData.Settings.IsCommandHistoryEnabled() then "enable" else "disable"
                    dictionary["command_history"] <- new KeyValuePair<string, List<ConsoleColor>>(value, consoleColors)

                if dictionary.ContainsKey("default_format") = false then
                    let value = userData.Settings.DefaultFormat()
                    dictionary["default_format"] <- new KeyValuePair<string, List<ConsoleColor>>(value, consoleColors)

                if dictionary.ContainsKey("default_server") = false then
                    let value = if userData.Settings.DefaultServer() <> null then userData.Settings.DefaultServer() else "null"
                    dictionary["default_server"] <- new KeyValuePair<string, List<ConsoleColor>>(value, consoleColors)

                if dictionary.ContainsKey("video_history") = false then
                    let value = if userData.Settings.IsWatchHistoryEnabled() then "enable" else "disable"
                    dictionary["video_history"] <- new KeyValuePair<string, List<ConsoleColor>>(value, consoleColors)
                
                if dictionary.ContainsKey("subtitles") = false then
                    let value = if userData.Settings.AreSubtitlesEnabled() then "enable" else "disable"
                    dictionary["subtitles"] <- new KeyValuePair<string, List<ConsoleColor>>(value, consoleColors)
                if dictionary.ContainsKey("subtitle_language") = false then
                    let value = userData.Settings.SubtitleLanguage()
                    dictionary["subtitle_language"] <- new KeyValuePair<string, List<ConsoleColor>>(value, consoleColors)

                Prints.PrintDictionaryWithColor(dictionary)
                Console.WriteLine()
                0
            elif args[0] = "remove" then
                if args.Count < 2 then
                    -2// -2 indicating not enough args
                else
                    userData.RemoveSetting(args[1])
                    FileOperations.SaveUserData(userData)
                    0
            else
                -1
        member self.Match: Enums.MatchType = 
            Enums.MatchType.Equals
        member self.Name: string = 
            "settings"
        member self.RequiredArgCount: int = 
            1

