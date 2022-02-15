namespace TubeDl

open Thoth.Json.Net

type ChannelDetails =
    {
        Id : string
        OrganizationId : int
        Name : string
        Description : string
    }

module ChannelDetails =
    let decoder : Decoder<ChannelDetails> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                OrganizationId = get.Required.Field "organization_id" Decode.int
                Name = get.Required.Field "name" Decode.string
                Description = get.Required.Field "description" Decode.string
            }
        )

type VideoDetails =
    {
        Id : string
        ProfileId : int
        ChannelId : int
        Title : string
        /// API allows any possible string
        Episode : string option
        PublishedAt : System.DateTimeOffset
        LicenseCode : string
        DurationInMilliseconds : int
    }

module VideoDetails =
    let decoder : Decoder<VideoDetails> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                ProfileId = get.Required.Field "profile_id" Decode.int
                ChannelId = get.Required.Field "channel_id" Decode.int
                Title =
                    get.Required.Field "title" Decode.string
                    |> String.trim
                Episode = get.Optional.Field "episode" Decode.string
                PublishedAt = get.Required.Field "published_at" Decode.datetimeOffset
                LicenseCode = get.Required.Field "license_code" Decode.string
                DurationInMilliseconds = get.Required.Field "duration_in_milliseconds" Decode.int
            }
        )

    let duration videoDetails =
        videoDetails.DurationInMilliseconds
        |> TimeSpan.fromMilliseconds

[<RequireQualifiedAccess>]
type MediaType =
    | Mp4
    | Mp3
    | Jpeg
    | Png

module MediaType =
    let decoder : Decoder<MediaType> =
        fun path value ->
            let e = (path, BadPrimitive ("a media type", value))

            if Decode.Helpers.isString value then
                Decode.Helpers.asString value
                |> function
                    | "video/mp4" -> Ok MediaType.Mp4
                    | "audio/mp3" -> Ok MediaType.Mp3
                    | "image/jpeg" -> Ok MediaType.Jpeg
                    | "image/png" -> Ok MediaType.Png
                    | _ -> Error e
            else
                Error e

    let extension =
        function
        | MediaType.Mp4 -> "mp4"
        | MediaType.Mp3 -> "mp3"
        | MediaType.Jpeg -> "jpg"
        | MediaType.Png -> "png"

type VideoPath =
    {
        Path : string
        Name : string
        MediaType : MediaType
        ExpiresAt : System.DateTimeOffset
    }

module VideoPath =
    let decoder : Decoder<VideoPath> =
        Decode.object (fun get ->
            {
                Path = get.Required.Field "path" Decode.string
                Name = get.Required.Field "name" Decode.string
                MediaType = get.Required.Field "media_type" MediaType.decoder
                ExpiresAt = get.Required.Field "expires_at" Decode.datetimeOffset
            }
        )

    let path videoPath = videoPath.Path
