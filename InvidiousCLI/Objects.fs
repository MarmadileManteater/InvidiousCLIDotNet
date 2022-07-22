module MarmadileManteater.InvidiousCLI.Objects

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
        history.Add(video)
        // re-add the history object in case it does not exist and we just initialized it above
        _data["video_history"] <- history

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

    new() = UserData(new JObject())