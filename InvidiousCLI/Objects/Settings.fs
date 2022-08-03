namespace MarmadileManteater.InvidiousCLI.Objects

open Newtonsoft.Json.Linq
open System
open System.IO
open MarmadileManteater.InvidiousCLI.Environment

type Settings (hasSetting : Func<string, bool>, getSetting : Func<string, JToken>, setSetting : Action<string, JToken>) =
    
    member self.AreSubtitlesEnabled (?value : bool) : bool =
        if value = None then
            if hasSetting.Invoke("subtitles") then
                getSetting.Invoke("subtitles").Value<string>() = "enable"
            else
                false
        else
            let result = if value.Value then "enable" else "disable"
            setSetting.Invoke("subtitles", result)
            value.Value
    
    member self.DefaultFormat (?value : string) =
        if value = None then
            if hasSetting.Invoke("default_format") then
                getSetting.Invoke("default_format").Value<string>()
            else
                "360p"// Default Value
        else
            setSetting.Invoke("default_format", value.Value)
            value.Value
    
    member self.DefaultServer (?value : string) =
        if value = None then
            if hasSetting.Invoke("default_server") then
                getSetting.Invoke("default_server").Value<string>()
            else
                null// Default Value
        else
            setSetting.Invoke("default_server", value.Value)
            value.Value
    
    member self.DownloadPath (?value : string) =
        if value = None then
            if hasSetting.Invoke("download_path") then
                getSetting.Invoke("download_path").Value<string>()
            else
                Paths.Temp// Default Value
        else
            setSetting.Invoke("download_path", value.Value)
            value.Value

    member self.SubtitleLanguage (?value : string) =
        if value = None then
            if hasSetting.Invoke("subtitle_language") then
                getSetting.Invoke("subtitle_language").Value<string>()
            else
                "en"// Default Value
        else
            setSetting.Invoke("subtitle_language", value.Value)
            value.Value

    member self.IsCacheEnabled (?value : bool) : bool =
        if value = None then
            if hasSetting.Invoke("cache") then
                getSetting.Invoke("cache").Value<string>() = "enable"
            else
                true// Default Value
        else
            setSetting.Invoke("cache", if value.Value then "enable" else "disable")
            value.Value

    member self.IsCommandHistoryEnabled (?value : bool) : bool =
        if value = None then
            if hasSetting.Invoke("command_history") then
                getSetting.Invoke("command_history").Value<string>() = "enable"
            else
                false// Default Value
        else
            setSetting.Invoke("command_history", if value.Value then "enable" else "disable")
            value.Value

    member self.IsWatchHistoryEnabled (?value : bool) : bool =
        if value = None then
            if hasSetting.Invoke("video_history") then
                getSetting.Invoke("video_history").Value<string>() = "enable"
            else
                false// Default Value
        else
            setSetting.Invoke("video_history", if value.Value then "enable" else "disable")
            value.Value