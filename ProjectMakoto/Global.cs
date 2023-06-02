// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

global using Dapper;
global using DisCatSharp;
global using DisCatSharp.ApplicationCommands;
global using DisCatSharp.ApplicationCommands.Attributes;
global using DisCatSharp.ApplicationCommands.Context;
global using DisCatSharp.CommandsNext;
global using DisCatSharp.CommandsNext.Attributes;
global using DisCatSharp.CommandsNext.Converters;
global using DisCatSharp.Entities;
global using DisCatSharp.Enums;
global using DisCatSharp.EventArgs;
global using DisCatSharp.Interactivity;
global using DisCatSharp.Interactivity.Extensions;
global using DisCatSharp.Lavalink;
global using DisCatSharp.Lavalink.EventArgs;
global using DisCatSharp.Net;
global using DisCatSharp.Extensions.TwoFactorCommands;
global using DisCatSharp.Extensions.TwoFactorCommands.ApplicationCommands;
global using DisCatSharp.Extensions.TwoFactorCommands.Entities;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using MySql.Data.MySqlClient;
global using Newtonsoft.Json;
global using Octokit;
global using ProjectMakoto.Commands;
global using ProjectMakoto.Database;
global using ProjectMakoto.Entities;
global using ProjectMakoto.Enums;
global using ProjectMakoto.Events;
global using ProjectMakoto.Exceptions;
global using ProjectMakoto.Util;
global using QuickChart;
global using System;
global using System.Collections.Generic;
global using System.Data;
global using System.Diagnostics;
global using System.Drawing;
global using System.Globalization;
global using System.IO;
global using System.IO.Compression;
global using System.Linq;
global using System.Net;
global using System.Net.Http;
global using System.Runtime.InteropServices;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Threading.Tasks;
global using Xorog.Logger;
global using Xorog.ScoreSaber;
global using Xorog.ScoreSaber.Objects;
global using Xorog.UniversalExtensions;
global using Xorog.UniversalExtensions.Entities;
global using ProjectMakoto.Plugins;
global using ProjectMakoto.PrefixCommands;
global using ProjectMakoto.Util.SystemMonitor;
global using System.Collections;
global using System.Reflection;
global using ProjectMakoto.Util.JsonSerializers;
global using static ProjectMakoto.Util.Log;
global using static Xorog.Logger.Logger;
global using static Xorog.UniversalExtensions.UniversalExtensions;
global using static Xorog.UniversalExtensions.UniversalExtensionsEnums;
global using PermissionLevel = DisCatSharp.Enums.PermissionLevel;
global using User = ProjectMakoto.Entities.User;
global using FileMode = System.IO.FileMode;
global using LogLevel = Xorog.Logger.Enums.LogLevel;
global using RequestParameters = Xorog.ScoreSaber.Objects.RequestParameters;