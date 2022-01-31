namespace TubeDl

open Thoth.Json.Net

type ChannelDetails =
    {
        Id : string
        ProfileId : int
        ChannelId : int
        Title : string
        PublishedAtAt : System.DateTimeOffset
        LicenseCode : string
        DurationInMilliseconds : int
    }

module ChannelDetails =
    let decoder : Decoder<ChannelDetails> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                ProfileId = get.Required.Field "profile_id" Decode.int
                ChannelId = get.Required.Field "channel_id" Decode.int
                Title = get.Required.Field "title" Decode.string
                PublishedAtAt = get.Required.Field "published_at" Decode.datetimeOffset
                LicenseCode = get.Required.Field "license_code" Decode.string
                DurationInMilliseconds = get.Required.Field "duration_in_milliseconds" Decode.int
            }
        )
