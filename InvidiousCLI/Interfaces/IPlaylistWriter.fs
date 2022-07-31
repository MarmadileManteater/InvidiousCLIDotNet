namespace MarmadileManteater.InvidiousCLI.Interfaces

open System.Collections.Generic
open MarmadileManteater.InvidiousCLI.Objects
open MarmadileManteater.InvidiousCLI.Enums
open MarmadileManteater.InvidiousClient.Interfaces
open System
open MarmadileManteater.InvidiousClient.Objects.Data

type IPlaylistWriter =
    interface
    /// <summary>The executable name for the media player that this writer will be used for</summary>
    abstract ExecutableName : string
    /// <summary>
    /// Generates a playlist file from a playlist
    /// </summary>
    abstract GenerateFileFromPlaylist : playlist : InvidiousPlaylist * urls: IList<string> -> string
end
