# switchtube-dl

A simple CLI for downloading videos from SwitchTube.

This makes it a lot more convenient to consume the videos on-demand when you're offline.

[![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/panmau/switchtube-dl/issues)
[![Release](https://img.shields.io/github/release/panmau/switchtube-dl.svg?classes=badges)](https://github.com/lucassabreu/clockify-cli/releases/latest)
[![CI](https://github.com/panmau/switchtube-dl/actions/workflows/ci.yml/badge.svg)](https://github.com/panmau/switchtube-dl/actions/workflows/ci.yml)

> **Disclaimer**:
>
> This isn't yet thoroughly tested. It will get tested in-depth throughout the next academic term.
>
> Please report any issues that you find.

## Features

- [x] download videos from a video id
- [x] videos from channels are downloaded interactively

Installation [![Powered By: dotnet-releaser](https://img.shields.io/badge/powered%20by-dotnet--releaser-green)](https://github.com/xoofx/dotnet-releaser)
------------

### Linux

#### Arch Linux

Install it with your favourite AUR helper. For example:

```bash
pamac install switchtube-dl
# or:
yay -S switchtube-dl
```

#### Debian

Download the `.deb` package for your architecture from
the [latest release](https://github.com/panmau/switchtube-dl/releases/latest) and install it with your package manager.

### macOS

(Installation with Homebrew coming soon.)

```bash
  brew install panmau/panmau/switchtube-dl
```

### Windows

Download the [latest windows release](https://github.com/panmau/switchtube-dl/releases/latest) zip for your
architecture. If you aren't sure about your architecture, try it with `win-x64.zip`.

Unzip it and put the `.exe` in an appropriate place. For easier access, make sure that this place is contained in your
PATH variable.

## Getting Started

After installation generate an API Token from your [SwitchTube Profile](https://tube.switch.ch/access_tokens) and save
it to an appropriate place.

You will need this token for every download that you run.

## Usage

> **Please note** that you should be gentle when downloading videos in parallel as noted in the [official API docs](https://tube.switch.ch/api#accessing-the-web-service):
>
> `Please be gentle with the servers because hammering the web service will also decrease performance of the web site.`


<br>

```bash
USAGE: switchtube-dl [--help] [--video <video id>] [--channel <channel id>] --token <token> [--path <path>] [--force]

OPTIONS:

    --video, -v <video id>
                          Download type. Downloads a specific video. Prioritized if multiple download types are given
    --channel, -c <channel id>
                          Download type. Download videos from this channel
    --token, -t <token>   Token to access the SwitchTube API (mandatory). Generate a token at https://tube.switch.ch/access_tokens
    --path, -p <path>     Paths to download videos to (defaults to current dir)
    --force, -f           Overwrite already existing files
    --help                display this list of options.
```

### Download a video

```bash
switchtube-dl -v 123456 --token $SWITCHTUBE_TOKEN
```

### Download a channel

```bash
switchtube-dl -c 123456 --token $SWITCHTUBE_TOKEN
```

It will per default output a table of all videos in this channel:

```
Index │ Title              │ Duration │    Date
──────┼────────────────────┼──────────┼────────────
    1 │ Video A            │ 00:09:39 │ 2020-07-08
    2 │ Video B            │ 00:09:39 │ 2020-09-09
    3 │ Video C            │ 00:15:16 │ 2020-09-09
    4 │ Video D            │ 00:11:45 │ 2020-09-09
    5 │ Video E            │ 00:06:04 │ 2020-09-09
    6 │ Video F            │ 00:08:16 │ 2020-09-09
    7 │ Video G            │ 00:12:25 │ 2021-11-30
    8 │ Video H            │ 00:11:12 │ 2022-02-07
    9 │ Video I            │ 01:03:00 │ 2022-02-09
```

To choose which videos to download, specify indices of the videos separated by commas like: `1,4,5,6` if you want to
download the videos indexed 1,4,5,6 of the channel.

You can specify a range, for example `1-3,5,7-9` will download the videos at index 1, 2, 3, 5, 7, 8 and 9.

#### Global flags

##### Overwrite existing files

Per default the CLI exits when it notices that a file already exists under the same name. If it should overwrite it
instead use
`-f` or `--force`.

#### Provide the path

Per default the files are downloaded to the directory from where the CLI was invoked. If you want to provide a different
path use `-p` or `--path`. This path **must** be **absolute**

## Related

[Teletube](https://github.com/Fingertips/teletube): If you need upload access to SwitchTube

## Contributing

Any type of feedback, pull request or issue is welcome.

See `CONTRIBUTING.md` for ways to get started.

## License

[MIT](https://choosealicense.com/licenses/mit/)

