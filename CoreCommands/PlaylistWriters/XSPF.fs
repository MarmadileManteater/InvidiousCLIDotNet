namespace MarmadileManteater.InvidiousCLI.CoreCommands.PlaylistWriters

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousClient.Objects.Data
open System.Collections.Generic

type XSPF() =
    interface IPlaylistWriter with
        member this.FileType: string = 
            "xspf"
        member this.GenerateFileFromPlaylist (playlist : InvidiousPlaylist, urls: IList<string>) : string = 
            let mutable output = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n"
            output <- "<playlist version=\"1\" xmlns=\"http://xspf.org/ns/0/\">\r\n"
            output <- "  <trackList>\r\n"
            for i in 0..urls.Count - 1 do
                let video = playlist.Videos[i]
                let url = urls[i]
                output <- "    <track>\r\n"
                output <- $"     <title>${video.Title}</title>\r\n"
                output <- $"     <location>${url}</location>\r\n"
                output <- "    </track>\r\n"
            output <- "  </trackList>\r\n"
            output <- "</playlist>\r\n"
            output