﻿namespace MarmadileManteater.InvidiousCLI.CoreCommands.PlaylistWriters

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousClient.Objects.Data
open System.Collections.Generic

type WPL() =
    inherit IPlaylistWriter()
        override self.SupportedPlayers: IList<string> = 
            let results = new List<string>()
            results.Add("wmplayer")
            results
        override self.FileType: string = 
            "wpl"
        override self.GenerateFileFromPlaylist (playlist : InvidiousPlaylist, urls: IList<string>) : string = 
            let title = playlist.Title
            let author = playlist.Author
            let count = playlist.Videos.Count
            let mutable output = "<?wpl version=\"1.0\"?>\r\n"
            output <- "<smil>\r\n"
            output <- "  <head>\r\n"
            output <- "    <meta name=\"Generator\" content=\"Invidious CLI\" />\r\n"
            output <- $"    <meta name=\"ItemCount\" content=\"${count}\" />\r\n"
            output <- $"    <author>${author}</author>\r\n"
            output <- $"    <title>${title}</title>\r\n"
            output <- "  </head>\r\n"
            output <- "  <body>\r\n"
            output <- "    <seq>\r\n"
            for url in urls do
                output <- $"      <media src=\"${url}\" />\r\n"
            output <- "    </seq>\r\n"
            output <- "  </body>\r\n"
            output <- "</smil>\r\n"
            output