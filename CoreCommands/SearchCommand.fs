﻿namespace MarmadileManteater.InvidiousCLI.CoreCommands

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousClient.Interfaces
open MarmadileManteater.InvidiousClient.Extensions
open MarmadileManteater.InvidiousCLI.Objects
open System.Collections.Generic
open MarmadileManteater.InvidiousCLI.Functions
open System
open MarmadileManteater.InvidiousCLI.Enums

type SearchCommand() =
    inherit ICommand()
        override self.OnInit(pluginObjects : IList<IPluginObject>): unit = 
            ()
        override self.Description: string = 
            "Performs a search with the given arguments as the query"
        override self.Documentation: IEnumerable<string> = 
            [
                "@param query : string";
                "search {query}"
            ]
        override self.Execute(args: string[], userData: UserData, client: IInvidiousAPIClient, isInteractive: bool, processCommand: Action<string[],IInvidiousAPIClient,UserData,bool>): int = 
            // If there are enough arguments,
            let mutable query = ""
            let mutable i = 0
            for arg in args do
                query <- query + arg + " "
                i <- i + 1
            done
            // All of the arguments are the query.
            let results = client.SearchSync(query)
            for result in results do
                if result.IsVideo() then
                    let video = result.ToVideo()
                    Prints.PrintShortVideoInfo(video)
                elif result.IsChannel() then
                    let channel = result.ToChannel()
                    Prints.PrintChannelInfo(channel)
                elif result.IsPlaylist() then
                    let playlist = result.ToPlaylist()
                    Prints.PrintPlaylistInfo(playlist)
            done
            let mutable page = 0
            if isInteractive then
                let mutable hasControl = true
                while hasControl do
                    let input = System.Console.ReadLine()
                    let innerArguments = CLI.StringToArgumentList(input)
                    if innerArguments.Length > 0 then
                        let command = innerArguments[0]

                        if command = "next" then
                            page <- page + 1
                            let results = client.SearchSync(query, page)
                            for result in results do
                                if result.IsVideo() then
                                    let video = result.ToVideo()
                                    Prints.PrintShortVideoInfo(video)
                                elif result.IsChannel() then
                                    let channel = result.ToChannel()
                                    Prints.PrintChannelInfo(channel)
                                elif result.IsPlaylist() then
                                    let playlist = result.ToPlaylist()
                                    Prints.PrintPlaylistInfo(playlist)
                            done
                        elif command = "previous" then
                            page <- page - 1
                            if page > -1 then
                                let results = client.SearchSync(query, page)
                                for result in results do
                                    if result.IsVideo() then
                                        let video = result.ToVideo()
                                        Prints.PrintShortVideoInfo(video)
                                    elif result.IsChannel() then
                                        let channel = result.ToChannel()
                                        Prints.PrintChannelInfo(channel)
                                    elif result.IsPlaylist() then
                                        let playlist = result.ToPlaylist()
                                        Prints.PrintPlaylistInfo(playlist)
                                done
                            else
                                page <- 0
                                Prints.PrintAsColorNewLine("You are already on the first page. There is no previous page.", ConsoleColor.DarkYellow, Console.BackgroundColor)
                        else
                            processCommand.Invoke(CLI.StringToArgumentList(input), client, userData, isInteractive)
                            // return control to the main program
                            hasControl <- false
            0
        override self.Match: MatchType = 
            MatchType.Equals
        override self.Name: string = 
            "search"
        override self.RequiredArgCount: int = 
            1