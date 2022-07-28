namespace MarmadileManteater.InvidiousCLI

open System.Collections.Generic
open System
open System.Linq
open System.IO
open Newtonsoft.Json.Linq
open MarmadileManteater.InvidiousClient.Objects
open MarmadileManteater.InvidiousClient.Interfaces
open MarmadileManteater.InvidiousCLI.Objects
open MarmadileManteater.InvidiousCLI.Functions
open MarmadileManteater.InvidiousCLI.Interfaces

module Program =
    // This is the central entry point for all of the command processing.
    // It is recursive to handle commands that need to be parsed before processing.
    // If a user enters a link, it parses out the id for the video and runs
    // processCommand(["watch", "{videoId}"], client, userData, false) where {videoId} is id pulled out of the link.
    let rec ProcessCommand (args : IList<string>, client : IInvidiousAPIClient, userData : UserData, takeAdditionalInput : bool, commands : IList<ICommand>) =
        // Saving to command history
        if userData.Settings.IsCommandHistoryEnabled() then
            userData.AddToCommandHistory(String.Join(" ", args)) |> ignore
            FileOperations.SaveUserData(userData)

        if args.Count > 0 then
            let mutable pluginFound = false
            for command : ICommand in commands do
                if pluginFound = false then
                    if (command.Match = Enums.MatchType.Equals && command.Name = args[0]) 
                    || (command.Match = Enums.MatchType.StartsWith && args[0].StartsWith(command.Name))
                    || (command.Match = Enums.MatchType.EndsWith && args[0].EndsWith(command.Name)) then
                        pluginFound <- true
                        let innerArguments = new List<string>()
                        let includeFirstParameter = command.Match <> Enums.MatchType.Equals
                        let start = if includeFirstParameter then 0 else 1
                        for i in start..args.Count - 1 do
                            innerArguments.Add(args[i])
                        try
                            if innerArguments.Count >= command.RequiredArgCount then
                                let result = command.Execute(innerArguments, userData, client, takeAdditionalInput, fun innerArgs innerClient innerUserData innerIsInteractive -> ProcessCommand(innerArgs, innerClient, innerUserData, innerIsInteractive, commands))
                                if result < 0 then
                                    Prints.PrintAsColorNewLine("The command completed with an unsuccessful status code.", ConsoleColor.Red, Console.BackgroundColor)
                                if result = -2 then
                                    Prints.PrintAsColorNewLine("Not enough arguments were given for the selected command", ConsoleColor.Red, Console.BackgroundColor)
                                    Console.WriteLine()
                                    Prints.PrintCommandInfo(command, "")
                            else
                                Prints.PrintAsColorNewLine("Not enough arguments were given for the selected command", ConsoleColor.Red, Console.BackgroundColor)
                                Console.WriteLine()
                                Prints.PrintCommandInfo(command, "")
                        with
                            ex ->
                                Prints.PrintAsColorNewLine("The command did not complete.", ConsoleColor.Red, Console.BackgroundColor)
                                Prints.PrintAsColorNewLine(ex.Message, ConsoleColor.White, ConsoleColor.Red)
                                Console.WriteLine()
                                Prints.PrintCommandInfo(command, "")
            if args[0] = "help" then
                Prints.PrintAsColorNewLine("Commands:", ConsoleColor.White, Console.BackgroundColor)
                for command in commands do
                    Prints.PrintAsColorNewLine("  " + command.Name, ConsoleColor.White, Console.BackgroundColor)
                    Prints.PrintCommandInfo(command, "    ")
                done
            elif pluginFound = false then
                let command = args[0]
                Prints.PrintAsColorNewLine("\"" + command + "\" is not recognized as a command.", ConsoleColor.Red, Console.BackgroundColor)
    let FirstTimeSetupWrapper(commands : IList<ICommand>) =
        let FirstTimeSetup (userData : UserData) =
            Prints.PrintAsColorNewLine("Initializing . . . ", ConsoleColor.Blue, ConsoleColor.Black)
            let checkForExistingPlayer (playerExecutable : string, workingDirectory : string, arguments : string) =
                if File.Exists(playerExecutable) then
                    ProcessCommand(("media-player add" + " " + playerExecutable + " " + arguments).Split(" ").ToList<string>(), null, userData, false, commands)
            let vlcArguments = "--input-slave=\"{audio_stream}\" --sub-file=\"{subtitle_file}\" --meta-title=\"{title}\""
            let mpvArguments = "--sub-file=\"{subtitle_file}\" --title=\"{title}\" --lavfi-complex='[vid1] [vid2] vstack [vo]' "
            // #region Windows
            checkForExistingPlayer("C:\Program Files (x86)\Windows Media Player\wmplayer.exe", "C:\Program Files (x86)\Windows Media Player\\", "")
            checkForExistingPlayer("C:/Program Files/VideoLAN/VLC/vlc.exe", "C:/Program Files/VideoLAN/VLC/", vlcArguments)
            // #endregion
            // #region WSL
            checkForExistingPlayer("/mnt/c/Program Files (x86)/Windows Media Player/wmplayer.exe", "/mnt/c/Program Files (x86)/Windows Media Player/", "")
            checkForExistingPlayer("/mnt/c/Program Files/VideoLAN/VLC/vlc.exe", "/mnt/c/Program Files/VideoLAN/VLC/", vlcArguments)
            // #endregion
            // #region Linux
            checkForExistingPlayer("/usr/bin/vlc", "/usr/bin/", vlcArguments)
            // #endregion
            let mutable command = new List<string>()
            command.Add("media-player")
            command.Add("list")
            ProcessCommand(command, null, userData, true, commands) |> ignore
            let mutable input = ""
            if userData.MediaPlayers.Count > 1 then
                Prints.PrintAsColor("Enter the index of the media player you would like to use:", ConsoleColor.Yellow, Console.BackgroundColor)
                input <- System.Console.ReadLine()
            if input = "" then
                Prints.PrintAsColor("Setup a media player? [", ConsoleColor.DarkYellow, Console.BackgroundColor)
                Prints.PrintAsColor("Y", ConsoleColor.Green, Console.BackgroundColor)
                Prints.PrintAsColor("/", ConsoleColor.DarkYellow, Console.BackgroundColor)
                Prints.PrintAsColor("N", ConsoleColor.Red, Console.BackgroundColor)
                Prints.PrintAsColorNewLine("]", ConsoleColor.DarkYellow, Console.BackgroundColor)
                // Loop for validating that the user is either agreeing or declining
                let mutable setupMediaPlayer = System.Console.ReadLine()
                while setupMediaPlayer <> "Y" && setupMediaPlayer <> "N" do
                    Prints.PrintAsColor("Setup a media player? [", ConsoleColor.DarkYellow, Console.BackgroundColor)
                    Prints.PrintAsColor("Y", ConsoleColor.Green, Console.BackgroundColor)
                    Prints.PrintAsColor("/", ConsoleColor.DarkYellow, Console.BackgroundColor)
                    Prints.PrintAsColor("N", ConsoleColor.Red, Console.BackgroundColor)
                    Prints.PrintAsColorNewLine("]", ConsoleColor.DarkYellow, Console.BackgroundColor)
                    setupMediaPlayer <- System.Console.ReadLine()
                done
                if setupMediaPlayer = "Y" then
                    // If the user wants to setup a media player,
                    Prints.PrintAsColor("Media Player executable path:", ConsoleColor.DarkYellow, ConsoleColor.Black)
                    let executablePath = System.Console.ReadLine()
                    command <- new List<string>()
                    command.Add("media-player")
                    command.Add("add")
                    command.Add(executablePath)
                    ProcessCommand(command, null, userData, true, commands)
            else
                let result = try input |> int |> Nullable<int>
                                with:? FormatException ->
                                new Nullable<int>()
                if result.HasValue then
                    if userData.MediaPlayers.Count > result.Value then
                        command <- new List<string>()
                        command.Add("media-player")
                        command.Add("set-primary")
                        command.Add(input)
                        ProcessCommand(command, null, userData, true, commands)
        FirstTimeSetup
    [<EntryPoint>]
    let Main(args) =
        let pluginPaths = new List<string>()
        // try to look for core commands in these locations
        pluginPaths.Add(@"..\..\..\..\CoreCommands\bin\Debug\net6.0\CoreCommands.dll")
        pluginPaths.Add(@"CoreCommands/CoreCommands.dll")
        // Paths to plugins to load.
        let commands : IList<ICommand> = 
            let result = new List<ICommand>();
            for pluginPath in pluginPaths do
                try
                    let pluginAssembly = Plugins.LoadPlugin(pluginPath)
                    let resultingCommands = Plugins.CreateCommands(pluginAssembly)
                    for command in resultingCommands do
                        result.Add(command)
                with
                    ex -> ex |> ignore
            result
        let mutable client = new InvidiousAPIClient()
        let mutable defaultServer = null
        let mutable cacheEnabled = true
        let mutable settingChanged = false
        if args.Length = 0 then
            // Running with no arguments
            // interactive prompt
            Prints.PrintGreeting()
            let userData = FileOperations.GetExistingUserData(FirstTimeSetupWrapper(commands))
            while true do
                if defaultServer <> userData.Settings.DefaultServer() then
                    defaultServer <- userData.Settings.DefaultServer()
                    settingChanged <- true
                if cacheEnabled <> userData.Settings.IsCacheEnabled() then
                    cacheEnabled <- userData.Settings.IsCacheEnabled()
                    settingChanged <- true
                if settingChanged then
                    client <- new InvidiousAPIClient(cacheEnabled, defaultServer)
                    settingChanged <- false
                let input = System.Console.ReadLine()
                ProcessCommand(input.Split(" "), client, userData, true, commands) |> ignore
            done
            0
        else
            // non interactive prompt
            let mutable userData = new UserData()
            // If the user data doesn't exist,
            // Don't run first time setup because it will trigger an interactive prompt
            if File.Exists(Paths.UserDataPath) then
                userData <- FileOperations.GetExistingUserData(FirstTimeSetupWrapper(commands))
                if defaultServer <> userData.Settings.DefaultServer() then
                    defaultServer <- userData.Settings.DefaultServer()
                if cacheEnabled <> userData.Settings.IsCacheEnabled() then
                    cacheEnabled <- userData.Settings.IsCacheEnabled()
                client <- new InvidiousAPIClient(cacheEnabled, defaultServer)
            ProcessCommand(args, client, userData, false, commands) |> ignore
            0
