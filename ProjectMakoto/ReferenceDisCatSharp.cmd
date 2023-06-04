@echo off
echo Removing Nuget Packages..
echo.
dotnet remove package DisCatSharp
dotnet remove package DisCatSharp.ApplicationCommands
dotnet remove package DisCatSharp.CommandsNext
dotnet remove package DisCatSharp.Common
dotnet remove package DisCatSharp.Configuration
dotnet remove package DisCatSharp.Experimental
dotnet remove package DisCatSharp.Interactivity
dotnet remove package DisCatSharp.Lavalink
dotnet remove package DisCatSharp.VoiceNext
dotnet remove package DisCatSharp.VoiceNext.Natives
echo.
echo Adding Local Projects..
echo.
dotnet sln add ..\..\DisCatSharp\DisCatSharp\DisCatSharp.csproj
dotnet sln add ..\..\DisCatSharp\DisCatSharp.ApplicationCommands\DisCatSharp.ApplicationCommands.csproj
dotnet sln add ..\..\DisCatSharp\DisCatSharp.CommandsNext\DisCatSharp.CommandsNext.csproj
dotnet sln add ..\..\DisCatSharp\DisCatSharp.Common\DisCatSharp.Common.csproj
dotnet sln add ..\..\DisCatSharp\DisCatSharp.Configuration\DisCatSharp.Configuration.csproj
dotnet sln add ..\..\DisCatSharp\DisCatSharp.Experimental\DisCatSharp.Experimental.csproj
dotnet sln add ..\..\DisCatSharp\DisCatSharp.Interactivity\DisCatSharp.Interactivity.csproj
dotnet sln add ..\..\DisCatSharp\DisCatSharp.Lavalink\DisCatSharp.Lavalink.csproj
dotnet sln add ..\..\DisCatSharp\DisCatSharp.VoiceNext\DisCatSharp.VoiceNext.csproj
dotnet sln add ..\..\DisCatSharp\DisCatSharp.VoiceNext.Natives\DisCatSharp.VoiceNext.Natives.csproj
echo.
echo Adding Project References..
echo.
dotnet add reference ..\..\DisCatSharp\DisCatSharp\DisCatSharp.csproj
dotnet add reference ..\..\DisCatSharp\DisCatSharp.ApplicationCommands\DisCatSharp.ApplicationCommands.csproj
dotnet add reference ..\..\DisCatSharp\DisCatSharp.CommandsNext\DisCatSharp.CommandsNext.csproj
dotnet add reference ..\..\DisCatSharp\DisCatSharp.Common\DisCatSharp.Common.csproj
dotnet add reference ..\..\DisCatSharp\DisCatSharp.Configuration\DisCatSharp.Configuration.csproj
dotnet add reference ..\..\DisCatSharp\DisCatSharp.Experimental\DisCatSharp.Experimental.csproj
dotnet add reference ..\..\DisCatSharp\DisCatSharp.Interactivity\DisCatSharp.Interactivity.csproj
dotnet add reference ..\..\DisCatSharp\DisCatSharp.Lavalink\DisCatSharp.Lavalink.csproj
dotnet add reference ..\..\DisCatSharp\DisCatSharp.VoiceNext\DisCatSharp.VoiceNext.csproj
dotnet add reference ..\..\DisCatSharp\DisCatSharp.VoiceNext.Natives\DisCatSharp.VoiceNext.Natives.csproj