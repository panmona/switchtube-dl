namespace TubeDl

open Thoth.Json.Net

type ChannelDetails =
    {
        Id: string
        OrganizationId: int
        Name: string
        Description: string
        Category: string
        Language: string
    }

module ChannelDetails =
    let decoder : Decoder<ChannelDetails> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                OrganizationId = get.Required.Field "organization_id" Decode.int
                Name = get.Required.Field "name" Decode.string
                Description = get.Required.Field "description" Decode.string
                Category = get.Required.Field "category" Decode.string
                Language = get.Required.Field "language" Decode.string
            }
        )

type ChannelVideo =
    {
        Id : string
        ProfileId : int
        ChannelId : int
        Title : string
        PublishedAtAt : System.DateTimeOffset
        LicenseCode : string
        DurationInMilliseconds : int
    }

module ChannelVideo =
    let decoder : Decoder<ChannelVideo> =
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


type VideoDetails =
    {
        Id: string
        ProfileId: int
        ChannelId: int
        Title: string
        PublishedAt: System.DateTimeOffset
        LicenseCode: string
        DurationInMilliseconds: int
    }

module VideoDetails =
    let decoder : Decoder<VideoDetails> =
        Decode.object (fun get ->
            {
                Id = get.Required.Field "id" Decode.string
                ProfileId = get.Required.Field "profile_id" Decode.int
                ChannelId = get.Required.Field "channel_id" Decode.int
                Title = get.Required.Field "title" Decode.string
                PublishedAt = get.Required.Field "published_at" Decode.datetimeOffset
                LicenseCode = get.Required.Field "license_code" Decode.string
                DurationInMilliseconds = get.Required.Field "expires_at" Decode.int
            }
        )

type VideoPath =
    {
        Path: string
        Name: string
        MediaType: string
        ExpiresAt: System.DateTimeOffset
    }

module VideoPath =
    let decoder : Decoder<VideoPath> =
        Decode.object (fun get ->
            {
                Path = get.Required.Field "path" Decode.string
                Name = get.Required.Field "name" Decode.string
                MediaType = get.Required.Field "media_type" Decode.string
                ExpiresAt = get.Required.Field "expires_at" Decode.datetimeOffset
            }
        )
