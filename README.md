<h1 align="center">Ichigo</h1>
<p align="center"><img src="ProjectIchigo/Assets/Prod.png" width=250 align="center"></p>
<p align="center" style="font-weight:bold;">A feature packed discord bot!</p>
<a href="#getting-ichigo" ><p align="center"><img src="ProjectIchigo/Assets/AddToServer.png" width=350 align="center"></p></a>

<p align="center"><img src="https://github.com/Fortunevale/ProjectIchigo/actions/workflows/build.yml/badge.svg" align="center"> <img src="https://github.com/Fortunevale/ProjectIchigo/actions/workflows/typos.yml/badge.svg" align="center"></p>
<p align="center"><img src="https://img.shields.io/github/contributors/Fortunevale/ProjectIchigo" align="center"> <img src="https://img.shields.io/github/issues-raw/Fortunevale/ProjectIchigo" align="center"></p>
<p align="center"><img src="https://wakatime.com/badge/github/Fortunevale/ProjectIchigo.svg" align="center"></p>

<p align="center"><img src="https://img.shields.io/github/stars/Fortunevale/ProjectIchigo?style=social" align="center"> <img src="https://img.shields.io/github/watchers/Fortunevale/ProjectIchigo?style=social" align="center"></p>

## What is Ichigo?

**Ichigo is a multi-purpose Discord Bot written in C# using .NET 6.**
<br></br>
Ichigo has a lot of features, current notable features are:
- **No premium features**. (This may change in the future, it'll depend on how viable a hosting this bot is without them. The source code itself will always stay available and you could simply host your own Ichigo instance.)
<br></br>
- **Music Playback**
- Customizable **protection against phishing** and other malicious websites, with little to no false positives.
- Easy to set up **Reaction Roles**.
- An easy to use **emoji and sticker stealer**.
- A **Bump Reminder** with a subscriber role to never miss bumping your server.
<br></br>
- Important **Moderation Features** such as a **detailed Actionlog**, commands to quickly clean up the chat(s) like `purge` or `guild-purge` and more.
- Quick and Easy **Message Translation** through the `Apps` Context Menu.
- **[ScoreSaber API](https://scoresaber.com)** Integration.
- An **experience** system with **role rewards**.
- **Social Commands** like `hug`, `pat` and a few more.
- **Automatic Nickname Normalization**, allowing quickly mentioning people with non-standard characters in their usernames.
- **Invite Tracking** so you can track a Raid's origin with just a few commands***.
<br></br>
- A system to **backup** user's **roles** and **nickname** when they leave*.
- **Custom Embed Creator** within Discord.
- **Embeds** for **message links** and **github code**.
- **Automatic Thread Unarchiving**, allowing threads to stay open for as long as you want them to.
- **Automatic Crossposting** so you can have automatic feeds in announcement channels.
- Additional Privacy and Security Features such as the **In-Voice Chat Privacy** or the **automatic bot/user token invalidation**.
<br></br>
##### \* Roles with any significant Permissions like Administrator won't be re-applied. In addition, if the user hasn't been on the server for more than 60 days, neither the roles nor the nickname will be reapplied. Also the `clearbackup` command gives moderators ability to remove stored roles.

##### \** A guild-purge is similar to a purge command. However, instead of scanning just one channel for messages by the specified user, it scans all channels.

##### \*** This depends on how users can join your server. If they join through invites by, for example, Disboard or through the Vanity Invite, it won't be as easy to track them down.
<br></br>
## Getting Ichigo

## [Click here to invite the bot](https://discord.com/api/oauth2/authorize?client_id=947716263394824213&permissions=8&scope=bot%20applications.commands)

- Phishing Protection is enabled by default, people will be banned if they send a link known to be malicious. To change this, run `/phishing config`.
- Automatic User/Bot Token invalidation is turned on by default. If you don't know what this means, just leave it on. If you know what this means and you don't want this happen, run `/tokendetection config` to disable it.
- Every new server is automatically opted into a global ban system. When someone is known to break Discord's TOS or Community Guidelines, they'll be banned on join or when the ban happens. They will not be banned when the bot is freshly added to your server. To change this behaviour you can use `/join config`.
- You can join a support server [here](https://s.aitsys.dev/ichigoguild).
<br></br>
## Building, Debugging and Deployment

### Prerequisites

- [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- The mentioned dependencies [below](#required-dependencies-that-you-cant-get-on-nuget)
- Your favourite C# IDE. My personal choice is [Visual Studio Community 2022](https://visualstudio.microsoft.com/vs/community/).

### Required Dependencies that you can't get on nuget:

- [Xorog.ScoreSaber](https://github.com/TheXorog/Xorog.ScoreSaber)
- [Xorog.Logger](https://github.com/TheXorog/Xorog.Logger)
- [Xorog.UniversalExtensions](https://github.com/TheXorog/Xorog.UniversalExtensions)
<br></br>
- [Lavalink](https://github.com/freyacodes/Lavalink)
- [LibreTranslate](https://github.com/LibreTranslate/LibreTranslate) (Please set up your own instance.)
- [A MySQL/MariaDB Database](https://dev.mysql.com/doc/mysql-installation-excerpt/8.0/en/general-installation-issues.html)

### Required APIs, Keys and Tokens

- [Discord Team](https://discord.com/developers/teams)
- [Discord Application](https://discord.com/developers/applications/)
   - The Application must be in previously mentioned Discord Team. The staff-check requires this.
- [Kawaii API Key](https://kawaii.red/)
   - This is mainly for social commands like `hug`, `cuddle`, `pat` and so on.
<br></br>
- [Github Account](https://github.com/) with a [Personal Access Token](https://github.com/settings/tokens) for your Github Account
   - The personal access token should have the `repo` permissions.
   - This is required for the `/dev_tools create-issue` command.
<br></br>
## Credits

- [DisCatSharp](https://github.com/Aiko-IT-Systems/DisCatSharp) by Aiko-IT-Systems
- [Lavalink](https://github.com/freyacodes/Lavalink) by freyacodes
- [LibreTranslate](https://github.com/LibreTranslate/LibreTranslate) by LibreTranslate
#### Phishing Link Repositories
- [discord-tokenlogger-link-list](https://github.com/nikolaischunk/discord-tokenlogger-link-list/) by nikolaischunk
- [links](https://github.com/DevSpen/links/) by DevSpen
- [SteamScamSites](https://github.com/PoorPocketsMcNewHold/SteamScamSites/) by PoorPocketsMcNewHold
- [fluffy-blocklist](https://github.com/sk-cat/fluffy-blocklist/) by sk-cat
- [videogame-scam-blocklist](https://github.com/Vytrah/videogame-scam-blocklist/) by Vytrah
