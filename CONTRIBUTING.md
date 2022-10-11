<h1 align="center">Ichigo</h1>
<p align="center"><img src="ProjectIchigo/Assets/Prod.png" width=250 align="center"></p>
<p align="center" style="font-weight:bold;">A feature packed discord bot!</p>
<a href="#getting-ichigo" ><p align="center"><img src="ProjectIchigo/Assets/AddToServer.png" width=350 align="center"></p></a>

<p align="center"><img src="https://github.com/Fortunevale/ProjectIchigo/actions/workflows/build.yml/badge.svg" align="center"> <img src="https://github.com/Fortunevale/ProjectIchigo/actions/workflows/typos.yml/badge.svg" align="center"></p>
<p align="center"><img src="https://img.shields.io/github/contributors/Fortunevale/ProjectIchigo" align="center"> <img src="https://img.shields.io/github/issues-raw/Fortunevale/ProjectIchigo" align="center"></p>
<p align="center"><img src="https://wakatime.com/badge/github/Fortunevale/ProjectIchigo.svg" align="center"></p>

<p align="center"><img src="https://img.shields.io/github/stars/Fortunevale/ProjectIchigo?style=social" align="center"> <img src="https://img.shields.io/github/watchers/Fortunevale/ProjectIchigo?style=social" align="center"></p>

# Step by Step Guide on how to set up a Development Environment

1. Install the following applications. These should get you started with a basic environment for C# development.
    - [Github Desktop](https://desktop.github.com/)
    - [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
        - Select .NET desktop development
2. Log into your Github Account on Github Desktop and clone the following repositories:
    - `Fortunevale/ProjectIchigo`
    - `Fortunevale/Xorog.ScoreSaber`
    - `Fortunevale/Xorog.Logger`
    - `Fortunevale/Xorog.UniversalExtensions`
3. With this completed, you can already start developing for Ichigo. To be able to debug Ichigo, follow the guide below.

## Running Ichigo with all necessary dependencies

1. Install the following applications.
    - [Lavalink](https://github.com/freyacodes/Lavalink)
        - **You need [Java](https://jdk.java.net/18/) to run Lavalink**
        - To learn how to setup Lavalink, you can check out **[this handy article](https://docs.dcs.aitsys.dev/articles/modules/audio/lavalink/setup.html)** made by the DisCatSharp Team.
    - [LibreTranslate](https://github.com/LibreTranslate/LibreTranslate)
        - **The easiest way of setting up an instance of LibreTranslate is to use [Docker](https://www.docker.com/)**
        - After installing Docker, run: `docker run -ti --rm -p 5000:5000 libretranslate/libretranslate`
    - [MariaDB](https://mariadb.com/kb/en/installing-mariadb-msi-packages-on-windows/)
        - Remember the password you set up, you'll need it.
        - After installing MariaDB, create 2 new databases: One for the main tables (guilds, users, scam_urls, etc.) and one for server members.

2. Create an account on the following sites:
    - [Discord](https://discord.com)
        - Create a new [Discord Team](https://discord.com/developers/teams) and add a new [Discord Application](https://discord.com/developers/applications/) to the previously created Team.
        - Add a Bot to the Application and note down the Bot Token.
        - The Bot requires all Intents.
    - [kawaii.red](https://kawaii.red/)
        - You'll find your Token on your User Dashboard.
    - [Github](https://github.com/)
        - If you want to use `/dev_tools create-issue`, create a [Personal Access Token](https://github.com/settings/tokens) to your Github Account.
3. Build and run Ichigo until the console says something like "Config reloaded".
4. Open the `config.json` in the path you built Ichigo in (usually `bin/Debug/`) and put in all values.
5. You're all set.

## Useful resources for development

- [DisCatSharp Documentation](https://docs.dcs.aitsys.dev/articles/preamble.html)