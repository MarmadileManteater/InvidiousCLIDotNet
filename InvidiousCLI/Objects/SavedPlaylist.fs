namespace MarmadileManteater.InvidiousCLI.Objects

open Newtonsoft.Json.Linq
open MarmadileManteater.InvidiousClient.Objects.Data
open System.Collections.Generic
open MarmadileManteater.InvidiousClient.Extensions

type SavedPlaylist(data) =
    let _data : JObject = data
    member self.Id : string =
        if _data.ContainsKey("id") then _data["id"].Value<string>() else ""
    member self.Title : string =
        if _data.ContainsKey("title") then _data["title"].Value<string>() else ""
    member self.Author : string =
        if _data.ContainsKey("author") then _data["author"].Value<string>() else ""
    member self.AuthorId : string =
        if _data.ContainsKey("author_id") then _data["author_id"].Value<string>() else ""
    member self.Videos : IList<InvidiousChannelVideo> =
        let result = new List<InvidiousChannelVideo>()
        let videos = if _data.ContainsKey("videos") then _data["videos"].Value<JArray>() else new JArray()
        for video in videos do
            result.Add(video.Value<JObject>().ToVideo())
        result
    member self.GetData () =
        _data