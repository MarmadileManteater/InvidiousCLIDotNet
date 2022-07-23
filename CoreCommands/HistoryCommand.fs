namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousCLI.Enums
open System.Collections.Generic
open MarmadileManteater.InvidiousCLI.Objects
open MarmadileManteater.InvidiousClient.Interfaces
open System
open MarmadileManteater.InvidiousCLI.Functions

type HistoryCommand() =
    interface ICommand with
        member self.Description: string = 
            "View the video history for this user data"
        member self.Documentation: System.Collections.Generic.IEnumerable<string> = 
            let results = new List<string>()
            results.Add("history list")
            results
        member self.Execute(args: IList<string>, userData: UserData, client: IInvidiousAPIClient, isInteractive : bool, processCommand : Action<IList<string>, IInvidiousAPIClient, UserData, bool>): int = 
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
                        let innerArguments = input.Split(" ")
                        if innerArguments.Length > 0 then
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
                                processCommand.Invoke(input.Split(" "), client, userData, isInteractive)
                                // return control to the main program
                                hasControl <- false
            elif args[0] = "clear" then
                Console.WriteLine()
            0
        member self.Match: MarmadileManteater.InvidiousCLI.Enums.MatchType = 
            MatchType.Equals
        member self.Name: string = 
            "history"
        member self.RequiredArgCount: int = 
            1