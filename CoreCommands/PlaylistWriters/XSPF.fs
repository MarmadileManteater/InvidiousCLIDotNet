namespace MarmadileManteater.InvidiousCLI.CoreCommands.PlaylistWriters

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousClient.Objects.Data
open System.Collections.Generic

type XSPF() =
    inherit IPlaylistWriter()
        override self.SupportedPlayers: IList<string> = 
            let results = new List<string>()
            results.Add("vlc")
            results
        override self.FileType: string = 
            "xspf"
        override self.GenerateFileFromPlaylist (playlist : InvidiousPlaylist, urls: IList<string>) : string = 
            let mutable output = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n"
            output <- output + "<playlist version=\"1\" xmlns=\"http://xspf.org/ns/0/\">\r\n"
            output <- output + "  <trackList>\r\n"
            for i in 0..urls.Count - 1 do
                let video = playlist.Videos[i]
                let url = urls[i]
                output <- output + "    <track>\r\n"
                let title = video.Title.Replace("&", "")
                output <- output + $"     <title>{title}</title>\r\n"
                let url = url.Replace("&", "&amp;")
                output <- output + $"     <location>{url}</location>\r\n"
                output <- output + "    </track>\r\n"
            output <- output + "  </trackList>\r\n"
            output <- output + "</playlist>\r\n"
            output