namespace MarmadileManteater.InvidiousCLI.CoreCommands.PlaylistWriters

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
            output <- output + "<smil>\r\n"
            output <- output + "  <head>\r\n"
            output <- output + "    <meta name=\"Generator\" content=\"Invidious CLI\" />\r\n"
            output <- output + $"    <meta name=\"ItemCount\" content=\"{count}\" />\r\n"
            output <- output + $"    <author>{author}</author>\r\n"
            output <- output + $"    <title>{title}</title>\r\n"
            output <- output + "  </head>\r\n"
            output <- output + "  <body>\r\n"
            output <- output + "    <seq>\r\n"
            for url in urls do// Does not work fro non-downloaded playlists as of right now
                output <- output + $"      <media src=\"{url}\" />\r\n"
            output <- output + "    </seq>\r\n"
            output <- output + "  </body>\r\n"
            output <- output + "</smil>\r\n"
            output