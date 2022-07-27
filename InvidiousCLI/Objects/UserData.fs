namespace MarmadileManteater.InvidiousCLI.Objects

open System.Collections.Generic
open Newtonsoft.Json.Linq
open System
open Newtonsoft.Json
open MarmadileManteater.InvidiousClient.Objects.Data
open MarmadileManteater.InvidiousClient.Extensions

type UserData(data) =
    let _data : JObject = data
    member self.CommandHistory =
        let history = if _data.ContainsKey("command_history") then _data["command_history"].Value<JArray>() else new JArray()
        let results = new List<string>()
        for command in history do
            results.Add(command.Value<string>())
        done
        results

    member self.AddToCommandHistory (command : string) =
        let history = if _data.ContainsKey("command_history") then _data["command_history"].Value<JArray>() else new JArray()
        history.Add(command)
        // re-add the history object in case it does not exist and we just initialized it above
        _data["command_history"] <- history

    member self.VideoHistory =
        let history = if _data.ContainsKey("video_history") then _data["video_history"].Value<JArray>() else new JArray()
        let results = new List<InvidiousChannelVideo>()
        for command in history do
            results.Add(command.Value<JObject>().ToVideo())
        done
        results

    member self.AddToVideoHistory (video : JObject) =
        let history = if _data.ContainsKey("video_history") then _data["video_history"].Value<JArray>() else new JArray()
        history.Insert(0, video)
        // re-add the history object in case it does not exist and we just initialized it above
        _data["video_history"] <- history

    member self.ClearVideoHistory () =
        _data["video_history"] <- new JArray()

    member self.GetData () =
        _data

    member self.AddMediaPlayer (mediaPlayer : JObject) =
        let mediaPlayers = if _data["media_players"] <> null then _data["media_players"].Value<JArray>() else new JArray()
        mediaPlayers.Add(mediaPlayer)
        _data["media_players"] <- mediaPlayers

    member self.MediaPlayers =
        let mediaPlayers = if _data.ContainsKey("media_players") then _data["media_players"].Value<JArray>() else new JArray()
        let results = new List<JObject>()
        for player in mediaPlayers do
            results.Add(player.Value<JObject>())
        done
        results

    member self.SetPrimaryMediaPlayer (mediaPlayerId : int) =
        if self.MediaPlayers.Count > mediaPlayerId then
            _data["primary_media_player"] <- JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(mediaPlayerId))
        else
            raise(new Exception("Media player id \"" + mediaPlayerId.ToString() + "\" does not correlate to any known media player."))

    member self.GetPrimaryMediaPlayerId () =
        let primaryMediaPlayer = if _data["primary_media_player"] <> null then _data["primary_media_player"].Value<int>() else 0
        if self.MediaPlayers.Count > primaryMediaPlayer then
            primaryMediaPlayer
        else
            0

    member self.GetPrimaryMediaPlayer () =
        let primaryMediaPlayer = self.GetPrimaryMediaPlayerId()
        if self.MediaPlayers.Count > primaryMediaPlayer then
            self.MediaPlayers[primaryMediaPlayer]
        else
            new JObject()

    member self.GetSettingsAsDictionary () : IDictionary<string, JToken> =
        let results = new Dictionary<string, JToken>()
        let settings = if _data.ContainsKey("settings") then _data["settings"].Value<JObject>() else new JObject()
        for setting in settings do
            results[setting.Key] <- setting.Value
        results

    member self.HasSetting (key : string) : bool =
        let settings = if _data.ContainsKey("settings") then _data["settings"].Value<JObject>() else new JObject()
        _data["settings"] <- settings
        settings[key] <> null

    member self.GetSetting (key : string) : JToken =
        let settings = if _data.ContainsKey("settings") then _data["settings"].Value<JObject>() else new JObject()
        _data["settings"] <- settings
        settings[key]

    member self.SetSetting (key : string, value : JToken) =
        let settings = if _data.ContainsKey("settings") then _data["settings"].Value<JObject>() else new JObject()
        settings[key] <- value
        _data["settings"] <- settings

    member self.RemoveSetting (key : string) =
        let settings = if _data.ContainsKey("settings") then _data["settings"].Value<JObject>() else new JObject()
        let newSettings = new JObject()
        for setting in settings do
            if setting.Key <> key then
                newSettings[setting.Key] <- setting.Value
        _data["settings"] <- newSettings

    member self.Settings : Settings =
        let hasSetting = fun key -> self.HasSetting(key)
        let getSetting = fun key -> self.GetSetting(key)
        let setSetting = fun key value -> self.SetSetting(key, value)
        new Settings(hasSetting, getSetting, setSetting)

    new() = UserData(new JObject())