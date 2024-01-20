<h1 align="center">Makoto</h1>
<p align="center"><img src="ProjectMakoto/Assets/Prod.png" width=250 align="center"></p>
<p align="center" style="font-weight:bold;">A feature packed discord bot!</p>
<a href="#getting-makoto" ><p align="center"><img src="ProjectMakoto/Assets/AddToServer.png" width=350 align="center"></p></a>

<p align="center"><img src="https://github.com/Fortunevale/ProjectMakoto/actions/workflows/dev.yml/badge.svg?branch=dev" align="center">
<p align="center"><img src="https://img.shields.io/github/contributors/Fortunevale/ProjectMakoto" align="center"> <img src="https://img.shields.io/github/issues-raw/Fortunevale/ProjectMakoto" align="center"></p>
<p align="center"><img src="https://wakatime.com/badge/github/Fortunevale/ProjectMakoto.svg" align="center"></p>

<p align="center"><img src="https://img.shields.io/github/stars/Fortunevale/ProjectMakoto?style=social" align="center"> <img src="https://img.shields.io/github/watchers/Fortunevale/ProjectMakoto?style=social" align="center"></p>

## Developing Plugins

1. Download the latest version of Makoto [here](https://github.com/Fortunevale/ProjectMakoto/releases).

<p align="center"><img src="DocAssets/DownloadRelease1.png" width=600 align="center"/></p>

2. Download the example plugin's source code [here](https://github.com/Fortunevale/ProjectMakoto.Plugins.Example).

<p align="center"><img src="DocAssets/DownloadProject1.png" width=400 align="center"/></p>

3. Create a folder called `deps` in the root directory of the example plugin.

4. Drop all files of release zip archive into the `deps` folder.

5. Open the project.

6. Specify your Plugin's Name, Author and other details in `ExamplePlugin.cs`.
    - The comments should help you get started.
    - You can rename this file, project and everything else, inheriting the `BasePlugin` is what matters for Makoto to find and load your plugin.

<p align="center"><img src="DocAssets/ExamplePluginInfo1.png" width=600 align="center"/></p>

## Testing your plugin

You need to set up Makoto ([Guide](CONTRIBUTING.md#running-makoto-with-all-necessary-dependencies)). Running/Debugging Makoto with all necessary dependencies.

To run Makoto, you can instead use `dotnet run ProjectMakoto.dll` in the folder you saved Makoto to in Step 1 of Developing.