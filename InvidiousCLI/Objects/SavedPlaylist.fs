namespace MarmadileManteater.InvidiousCLI.Objects

open Newtonsoft.Json.Linq
open MarmadileManteater.InvidiousClient.Objects.Data
open System.Collections.Generic
open MarmadileManteater.InvidiousClient.Extensions

type SavedPlaylist(data) =
    let _data : JObject = data
    member self.Id : string =
        if _data.ContainsKey("playlistId") then _data["playlistId"].Value<string>() elif _data.ContainsKey("id") then _data["id"].Value<string>() else ""
    member self.Title : string =
        if _data.ContainsKey("title") then _data["title"].Value<string>() else ""
    member self.Author : string =
        if _data.ContainsKey("author") then _data["author"].Value<string>() else ""
    member self.AuthorId : string =
        if _data.ContainsKey("authorId") then _data["authorId"].Value<string>() else ""
    member self.Videos : IList<InvidiousChannelVideo> =
        let result = new List<InvidiousChannelVideo>()
        let videos = if _data.ContainsKey("videos") then _data["videos"].Value<JArray>() else new JArray()
        for video in videos do
            result.Add(video.Value<JObject>().ToVideo())
        result
    member self.DownloadFormats : IList<string> =
        let result = new List<string>()
        let jarray = if _data["downloadFormats"] <> null then _data["downloadFormats"].Value<JArray>() else new JArray()
        for entry in jarray do
            result.Add(entry.ToString())
        result
    member self.QualityFormats : IDictionary<string, string> =
        let result = new Dictionary<string, string>()
        let jObject = if _data["qualityFormats"] <> null then _data["qualityFormats"].Value<JObject>() else new JObject()
        for entry in jObject do
            result.Add(entry.Key.ToString(), entry.Value.ToString())
        result
    member self.AddQualityFormat(quality : string, itag : string) =
        let jObject = if _data["qualityFormats"] <> null then _data["qualityFormats"].Value<JObject>() else new JObject()
        jObject.Add(quality, itag);
        _data["qualityFormats"] <- jObject
    member self.AddDownloadFormat (format : string) =
        let jarray = if _data["downloadFormats"] <> null then _data["downloadFormats"].Value<JArray>() else new JArray()
        jarray.Add(format)
        _data["downloadFormats"] <- jarray
    member self.GetData () =
        _data