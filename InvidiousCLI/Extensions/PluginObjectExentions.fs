namespace MarmadileManteater.InvidiousCLI.Extensions

open System.Runtime.CompilerServices
open MarmadileManteater.InvidiousCLI.Interfaces

[<Extension>]
type PluginObjectExentions =
    [<Extension>]
    static member IsCommand (pluginObject: IPluginObject) : bool =
        pluginObject.ObjectType.Equals("Command")
    [<Extension>]
    static member IsPlaylistWriter (pluginObject: IPluginObject) : bool =
        pluginObject.ObjectType.Equals("PlaylistWriter")