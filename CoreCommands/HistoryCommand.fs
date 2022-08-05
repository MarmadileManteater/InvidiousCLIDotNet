namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousCLI.Enums
open System.Collections.Generic
open MarmadileManteater.InvidiousCLI.Objects
open MarmadileManteater.InvidiousClient.Interfaces
open System
open MarmadileManteater.InvidiousCLI.Functions
open System.Linq

type HistoryCommand() =
    inherit ICommand()
        override self.OnInit(pluginObjects : IList<IPluginObject>): unit = 
            ()
        override self.Description: string = 
            "View the video history for this user data"
        override self.Documentation: System.Collections.Generic.IEnumerable<string> = 
            let results = new List<string>()
            results.Add("history list")
            results.Add("history enable")
            results.Add("history disable")
            results.Add("history clear")
            results
        override self.Execute(args: IList<string>, userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<IList<string>,IInvidiousAPIClient,UserData,bool>): int = 
            if args[0] = "list" then
                let mutable page = 0
                let pageLength = 10
                let hasNextPage = if userData.VideoHistory.Count > (page + 1) * pageLength then true else false
                for video in userData.VideoHistory.GetRange(page * pageLength, if hasNextPage then pageLength else userData.VideoHistory.Count) do
                    Prints.PrintShortVideoInfo(video)
                done
                Console.WriteLine()
                if isInteractive && hasNextPage then
                    let mutable hasControl = true
                    while hasControl do
                        let input = System.Console.ReadLine()
                        let innerArguments = CLI.StringToArgumentList(input)
                        if innerArguments.Count > 0 then
                            let command = innerArguments[0]

                            if command = "next" then
                                if (page + 1) * pageLength <= userData.VideoHistory.Count then
                                    page <- page + 1
                                    let hasNextPage = if userData.VideoHistory.Count > (page + 1) * pageLength then true else false
                                    for video in userData.VideoHistory.GetRange(page * pageLength, if hasNextPage then pageLength else userData.VideoHistory.Count % pageLength) do
                                        Prints.PrintShortVideoInfo(video)
                                    done
                                else
                                    Prints.PrintAsColorNewLine("You are on the last page.", ConsoleColor.DarkYellow, Console.BackgroundColor)
                            elif command = "previous" then
                                page <- page - 1
                                if page > -1 then
                                    let hasNextPage = if userData.VideoHistory.Count > (page + 1) * pageLength then true else false
                                    for video in userData.VideoHistory.GetRange(page * pageLength, if hasNextPage then pageLength else userData.VideoHistory.Count) do
                                        Prints.PrintShortVideoInfo(video)
                                    done
                                else
                                    page <- 0
                                    Prints.PrintAsColorNewLine("You are already on the first page. There is no previous page.", ConsoleColor.DarkYellow, Console.BackgroundColor)
                            else
                                processCommand.Invoke(CLI.StringToArgumentList(input), client, userData, isInteractive)
                                // return control to the main program
                                hasControl <- false
            elif args[0] = "enable" then
                processCommand.Invoke(CLI.StringToArgumentList("settings set video_history enable"), client, userData, isInteractive)
                processCommand.Invoke(CLI.StringToArgumentList("settings get video_history"), client, userData, isInteractive)
            elif args[0] = "disable" then
                processCommand.Invoke(CLI.StringToArgumentList("settings set video_history disable"), client, userData, isInteractive)
                processCommand.Invoke(CLI.StringToArgumentList("settings get video_history"), client, userData, isInteractive)
            elif args[0] = "clear" then
                userData.ClearVideoHistory()
                FileOperations.SaveUserData(userData)
                Prints.PrintAsColorNewLine("Cleared all history!", ConsoleColor.Green, Console.BackgroundColor)
                Console.WriteLine()
            0
        override self.Match: MarmadileManteater.InvidiousCLI.Enums.MatchType = 
            MatchType.Equals
        override self.Name: string = 
            "history"
        override self.RequiredArgCount: int = 
            1