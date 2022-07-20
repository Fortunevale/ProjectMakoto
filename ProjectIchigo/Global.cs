// System

global using System;
global using System.Drawing;
global using System.Collections.Generic;
global using System.Linq;
global using System.Net.Http;
global using System.Threading;
global using System.Text.RegularExpressions;
global using System.Threading.Tasks;
global using System.Diagnostics;
global using System.Runtime.InteropServices;
global using System.IO;
global using System.Net;
global using System.Text;
global using System.Globalization;
global using System.IO.Compression;
global using System.Data.SQLite;
global using System.Data.Common;
global using System.Security.Cryptography;
global using System.Data;
global using System.ComponentModel.DataAnnotations;
global using Microsoft.Extensions.DependencyInjection;
global using System.Collections.ObjectModel;
global using Microsoft.Extensions.Logging;
global using System.Collections.Specialized;

// Database

global using MySql.Data.MySqlClient;

// QuickChart

global using QuickChart;

// DisCatSharp

global using DisCatSharp;
global using DisCatSharp.CommandsNext;
global using DisCatSharp.Entities;
global using DisCatSharp.Enums;
global using DisCatSharp.EventArgs;
global using DisCatSharp.ApplicationCommands;
global using DisCatSharp.ApplicationCommands.Attributes;
global using DisCatSharp.CommandsNext.Attributes;
global using DisCatSharp.Interactivity.Extensions;
global using DisCatSharp.Interactivity;
global using DisCatSharp.Net;
global using DisCatSharp.Lavalink;
global using DisCatSharp.Lavalink.EventArgs;
global using DisCatSharp.CommandsNext.Converters;

// Own Utils

global using ProjectIchigo.Commands;

global using ProjectIchigo.ApplicationCommands;
global using ProjectIchigo.PrefixCommands;

global using ProjectIchigo.Enums;
global using ProjectIchigo.Secrets;
global using ProjectIchigo.Entities;
global using ProjectIchigo.Entities.Afk;
global using ProjectIchigo.Entities.Database;
global using ProjectIchigo.Entities.Legacy;

global using ProjectIchigo.Database;

global using ProjectIchigo.Events;

global using ProjectIchigo.Util;

global using ProjectIchigo.Extensions;

global using ProjectIchigo.Exceptions;


global using Xorog.ScoreSaber;
global using Xorog.ScoreSaber.Objects;

global using static ProjectIchigo.Util.Log;

global using Xorog.UniversalExtensions;
global using static Xorog.UniversalExtensions.UniversalExtensions;
global using static Xorog.UniversalExtensions.UniversalExtensionsEnums;


global using Xorog.Logger;
global using Xorog.Logger.Entities;
global using Xorog.Logger.Enums;
global using static Xorog.Logger.Logger;
global using LogLevel = Xorog.Logger.Enums.LogLevel;

// Misc

global using Newtonsoft.Json;
global using Dapper;
global using Octokit;

// Octokit Aliases

global using FileMode = System.IO.FileMode;
global using RequestParameters = Xorog.ScoreSaber.Objects.RequestParameters;
global using PermissionLevel = DisCatSharp.PermissionLevel;