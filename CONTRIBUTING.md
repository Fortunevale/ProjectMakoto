<h1 align="center">Makoto</h1>
<p align="center"><img src="ProjectMakoto/Assets/Prod.png" width=250 align="center"></p>
<p align="center" style="font-weight:bold;">A feature packed discord bot!</p>

<p align="center"><img src="https://github.com/Fortunevale/ProjectMakoto/actions/workflows/dev.yml/badge.svg?branch=dev" align="center">
<p align="center"><img src="https://img.shields.io/github/contributors/Fortunevale/ProjectMakoto" align="center"> <img src="https://img.shields.io/github/issues-raw/Fortunevale/ProjectMakoto" align="center"></p>
<p align="center"><img src="https://wakatime.com/badge/github/Fortunevale/ProjectMakoto.svg" align="center"></p>

<p align="center"><img src="https://img.shields.io/github/stars/Fortunevale/ProjectMakoto?style=social" align="center"> <img src="https://img.shields.io/github/watchers/Fortunevale/ProjectMakoto?style=social" align="center"></p>

## Step by Step Guide on how to set up a Development Environment

1. Install the following applications. These should get you started with a basic environment for C# development.
    - [Git CLI](https://www.git-scm.com/downloads)
    - [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
        - Select .NET desktop development
2. Log into your Github Account with the Git CLI and clone the following repository:
    - `Fortunevale/ProjectMakoto`
        - To clone `ProjectMakoto` with it's submodules run: `git clone --recurse-submodules "https://github.com/Fortunevale/ProjectMakoto.git"`
    - _You can skip this step if you're developing a plugin._
3. With this completed, you can already start developing for Makoto. To be able to debug Makoto, follow the guide below.

## Running/Debugging Makoto with all necessary dependencies

1. Install the following:
    - [MariaDB Server](https://mariadb.org/download/)
        - After installing the MariaDB Server, create 2 new databases: One for the main tables (guilds, users, scam_urls, etc.) and one for server members.
        - You'll need a third database if you're using plugins.

2. Create an account on the following sites:
    - [Discord](https://discord.com)
        - Create a new [Discord Team](https://discord.com/developers/teams) and add a new [Discord Application](https://discord.com/developers/applications/) to the previously created team.
        - Add a bot to the application and note down the bot token.
        - Makoto currently requires the `Presence`, `Server Members` and `Message Content` Intents.
        - I recommend disabling the `Public Bot` Option so no one can add your development client to their server.
    - [AbuseIPDB API Key](https://www.abuseipdb.com/account)
        - After creating your account, you can create an api key [here](https://www.abuseipdb.com/account/api).
    - [Github](https://github.com/)
        - Create a [Personal Access Token](https://github.com/settings/tokens) to your Github Account.
        - The bot needs to be able to create issues and read your repository.
3. Build and run Makoto until the console says something like "Config reloaded".
4. Open the `config.json` in the path you built Makoto in (usually `bin/Debug/`) and put in all values.
5. You're all set.

## Useful resources for development

- [DisCatSharp Documentation](https://docs.dcs.aitsys.dev/articles/preamble.html)
- [Translation Documentation](TRANSLATING.md)
- [Plugin Documentation](PLUGINS.md)