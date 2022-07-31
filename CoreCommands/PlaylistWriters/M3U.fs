namespace MarmadileManteater.InvidiousCLI.CoreCommands.PlaylistWriters

open MarmadileManteater.InvidiousCLI.Interfaces
open MarmadileManteater.InvidiousClient.Objects.Data
open System.Collections.Generic

type M3U() =
    inherit IPlaylistWriter()
        override self.FileType: string = 
            "m3u"
        override self.GenerateFileFromPlaylist (playlist : InvidiousPlaylist, urls: IList<string>) : string = 
            let mutable output = "#EXTM3U\r\n"
            output <- "\r\n"
            for i in 0..urls.Count - 1 do
                let video = playlist.Videos[i]
                let url = urls[i]
                output <- $"#EXTINF:${video.LengthSeconds}, ${video.Author} - ${video.Title}</title>\r\n"
                output <- $"${url}\r\n"
                output <- "\r\n"
            output