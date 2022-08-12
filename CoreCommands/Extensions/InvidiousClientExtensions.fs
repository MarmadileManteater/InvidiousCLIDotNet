namespace MarmadileManteater.InvidiousCLI.CoreCommands.Extensions

open System.Collections.Generic
open System.Linq
open System.Runtime.CompilerServices
open MarmadileManteater.InvidiousClient.Interfaces
open MarmadileManteater.InvidiousClient.Objects.Data
open MarmadileManteater.InvidiousCLI.Functions
open System

[<Extension>]
type InvidiousClientExtensions =
    [<Extension>]
    static member FetchFormatStreams(client : IInvidiousAPIClient, videoId : string, quality : string, itag : string, isVideoHistoryEnabled : bool, ?subtitles : bool) =
        let fields = [ "formatStreams"; "adaptiveFormats"; "title" ].ToList()
        if subtitles.IsNone <> true then
            if subtitles.Value then
                fields.Add("captions")
        if isVideoHistoryEnabled then
            // All the fields useful for history
            fields.Add("videoId")
            fields.Add("lengthSeconds")
            fields.Add("author")
            fields.Add("authorId")
            fields.Add("videoThumbnails")
        try
            let videoObject = client.FetchVideoByIdSync(videoId, fields.ToArray())
            let formatStreams : IList<FormatStream> = videoObject.FormatStreams
            let mutable mediumQualityStreams = formatStreams.Where(fun stream -> ((itag = null && stream.Resolution = quality) || stream.Itag = itag))
            // this is a list of audio streams
            let secondaryStreams = formatStreams.Where(fun secondary -> (itag = null && secondary.Type.Contains("audio") && secondary.Type.Contains(",") <> true))
            if mediumQualityStreams.Count() = 0 then
                Prints.PrintAsColorNewLine("There was no format stream of matching quality found.", ConsoleColor.Yellow, Console.BackgroundColor)
                Prints.PrintAsColorNewLine("Opening the default format for the video instead", ConsoleColor.DarkYellow, Console.BackgroundColor)
                mediumQualityStreams <- formatStreams.Where(fun stream -> stream.Type.Contains(","))// just get the first audio video stream
                mediumQualityStreams <- mediumQualityStreams.Reverse()
            [ mediumQualityStreams; secondaryStreams ]
        with
            ex -> [ null ; null ]
