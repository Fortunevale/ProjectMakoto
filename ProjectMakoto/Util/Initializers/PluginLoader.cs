// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using ProjectMakoto.Entities.Plugins.Commands;

namespace ProjectMakoto.Util.Initializers;
internal sealed class PluginLoader
{
    private static string CachedUsings = "";

    internal static async Task LoadPlugins(Bot _bot)
    {
        if (_bot.status.LoadedConfig.EnablePlugins)
        {
            _logger.LogDebug("Loading Plugins..");
            List<string> pluginsToLoad = new();

            if (Directory.Exists("Plugins"))
                pluginsToLoad.AddRange(Directory.GetFiles("Plugins").Where(x => x.EndsWith(".dll")));

            foreach (var pluginPath in pluginsToLoad)
            {
                int count = 0;
                _logger.LogDebug("Loading Plugin from '{0}'", pluginPath);

                PluginLoadContext pluginLoadContext = new(pluginPath);
                var assembly = pluginLoadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginPath)));

                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(BasePlugin).IsAssignableFrom(type))
                    {
                        count++;
                        BasePlugin result = Activator.CreateInstance(type) as BasePlugin;
                        result.LoadedFile = new FileInfo(pluginPath);
                        _bot._Plugins.Add(Path.GetFileNameWithoutExtension(pluginPath), result);
                    }
                }

                if (count == 0)
                {
                    string availableTypes = string.Join(", ", assembly.GetTypes().Select(t => t.FullName));
                    _logger.LogWarn("Cannot load Plugin '{0}': Plugin Assembly does not contain type that inherits BasePlugin. Types found: {1}", assembly.GetName(), availableTypes);
                }


                _logger.LogInfo("Loaded Plugin from '{0}'", pluginPath);
            }

            _logger.LogInfo("Loaded {0} Plugins.", _bot.Plugins.Count);
        }

        foreach (var b in _bot.Plugins)
        {
            if (b.Value.Name.IsNullOrWhiteSpace())
            {
                _logger.LogWarn("Skipped loading Plugin '{0}': Missing Name.", b.Key);
                continue;
            }

            if (b.Value.Description.IsNullOrWhiteSpace())
            {
                _logger.LogWarn("Skipped loading Plugin '{0}': Missing Description.", b.Key);
                continue;
            }

            if (b.Value.Author.IsNullOrWhiteSpace())
            {
                _logger.LogWarn("Skipped loading Plugin '{0}': Missing Author.", b.Key);
                continue;
            }

            if (b.Value.AuthorId is null)
            {
                _logger.LogWarn("Skipped loading Plugin '{0}': Missing AuthorId.", b.Key);
                continue;
            }

            if (b.Value.Version is null)
            {
                _logger.LogWarn("Skipped loading Plugin '{0}': Missing Version.", b.Key);
                continue;
            }

            _logger.LogDebug("Initializing Plugin '{0}' ({1})..", b.Value.Name, b.Key);

            try
            {
                b.Value.Load(_bot);
                _logger.LogInfo("Initialized Plugin from '{0}': '{1}' (v{2}).", b.Key, b.Value.Name, b.Value.Version.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load Plugin from '{0}': '{1}' (v{2}).", b.Key, b.Value.Name, b.Value.Version.ToString());
                _logger.LogError("Exception", ex);
            }

            try
            {
                await b.Value.CheckForUpdates();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to check updates for '{PluginName}'", ex, b.Value.Name);
            }
        }
    }

    internal static async Task LoadPluginCommands(Bot _bot, CommandsNextExtension cNext, ApplicationCommandsExtension appCommands)
    {
        var applicationHash = UniversalExtensions.ComputeSHA256Hash(new FileInfo(Assembly.GetExecutingAssembly().Location));

        foreach (var plugin in _bot.Plugins)
        {
            try
            {
                var pluginHash = UniversalExtensions.ComputeSHA256Hash(plugin.Value.LoadedFile);
                Dictionary<Assembly, string> assemblyList = new();

                var pluginCommands = await plugin.Value.RegisterCommands();
                _bot._PluginCommands.Add(plugin.Key, pluginCommands.ToList());

                if (_bot.status.LoadedConfig.PluginCache.TryGetValue(plugin.Key, out var pluginInfo) &&
                    pluginHash == pluginInfo.LastKnownHash &&
                    applicationHash == _bot.status.LoadedConfig.DontModify.LastKnownHash &&
                    pluginInfo.CompiledCommands.All(x => File.Exists(x.Key)))
                {
                    _logger.LogInfo("Loading {0} Commands from Plugin from '{1}' ({2}) from compiled assemblies..", pluginInfo.CompiledCommands.Count, plugin.Value.Name, plugin.Value.Version.ToString());
                    
                    foreach (var b in pluginInfo.CompiledCommands)
                    {
                        PluginLoadContext pluginLoadContext = new(b.Key);
                        var assembly = pluginLoadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(b.Key)));

                        assemblyList.Add(assembly, b.Value);
                    }

                    RegisterAssemblies(_bot, cNext, appCommands, plugin.Value, assemblyList);
                    continue;
                }

                _bot.status.LoadedConfig.PluginCache.TryAdd(plugin.Key, new());
                pluginInfo = _bot.status.LoadedConfig.PluginCache[plugin.Key];

                pluginInfo.LastKnownHash = pluginHash;

                if (pluginInfo.CompiledCommands.Any())
                    _ = UniversalExtensions.CleanupFilesAndDirectories(new(), pluginInfo.CompiledCommands.Select(x => x.Key).ToList());

                pluginInfo.CompiledCommands = new();

                string classUsings = GetUsings();

                if (pluginCommands.IsNotNullAndNotEmpty())
                {
                    var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithOptimizationLevel(OptimizationLevel.Release);
                    var references = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && !x.Location.IsNullOrWhiteSpace()).Select(x => MetadataReference.CreateFromFile(x.Location));

                    _logger.LogInfo("Compiling {0} BasePluginCommands from Plugin from '{1}' ({2}).", pluginCommands.Count, plugin.Value.Name, plugin.Value.Version.ToString());

                    Dictionary<CSharpCompilation, CompilationData> compilationList = new();

                    foreach (var rawCommand in pluginCommands)
                    {
                        rawCommand.Registered = true;

                        if (rawCommand.SupportedCommands.Contains(PluginCommandType.PrefixCommand))
                            if (rawCommand.IsGroup)
                            {
                                var code =
                                $$"""
                                {{classUsings}}

                                [Group("{{rawCommand.Name}}"), CommandModule("{{rawCommand.Module}}"), Description("{{rawCommand.Description}}")]
                                public sealed class {{GetUniqueCodeCompatibleName()}} : {{nameof(BaseCommandModule)}}
                                {
                                    public {{nameof(Bot)}} _bot { private get; set; }

                                    // EntryPoint
                                }
                                """;

                                var IndexPath = $"// EntryPoint";
                                int InsertPosition = InsertPosition = code.IndexOf(IndexPath) + IndexPath.Length;

                                foreach (var rawSubCommand in rawCommand.SubCommands)
                                {
                                    rawSubCommand.Registered = true;

                                    if (rawSubCommand.Name.ToLower() == "help")
                                        continue;

                                    rawSubCommand.Parent = rawCommand;
                                    code = code.Insert(InsertPosition, GetGroupMethodCode(plugin, nameof(CommandContext), rawSubCommand, rawCommand));
                                }

                                if (rawCommand.UseDefaultHelp)
                                {
                                    code = code.Insert(InsertPosition,
                                    $$"""

                                    [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
                                    public async Task Help(CommandContext ctx)
                                        => PrefixCommandUtil.SendGroupHelp(_bot, ctx, "{{rawCommand.Name}}").Add(_bot.watcher, ctx);
                                    """);
                                }

                                _logger.LogTrace($"\n{code}");

                                compilationList.Add(CSharpCompilation.Create(GetUniqueCodeCompatibleName())
                                    .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(code))
                                    .AddReferences(references)
                                        .WithOptions(options), 
                                        new CompilationData("prefix_group", code, rawCommand));
                            }
                            else
                            {
                                var code =
                                $$"""
                                {{classUsings}}

                                public sealed class {{GetUniqueCodeCompatibleName()}} : {{nameof(BaseCommandModule)}}
                                {
                                    public {{nameof(Bot)}} _bot { private get; set; }

                                    {{GetSingleMethodCode(plugin, nameof(CommandContext), rawCommand)}}
                                }
                                """;

                                compilationList.Add(CSharpCompilation.Create(GetUniqueCodeCompatibleName())
                                    .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(code))
                                    .AddReferences(references)
                                        .WithOptions(options),
                                        new CompilationData("prefix_single", code, rawCommand));
                            }

                        if (rawCommand.SupportedCommands.Contains(PluginCommandType.SlashCommand))
                            if (rawCommand.IsGroup)
                            {
                                var code =
                                $$"""
                                {{classUsings}}

                                public sealed class {{GetUniqueCodeCompatibleName()}} : {{nameof(ApplicationCommandsModule)}}
                                {
                                    [SlashCommandGroup("{{rawCommand.Name}}", "{{rawCommand.Description}}"{{(rawCommand.RequiredPermissions is null ? "" : $", {(long)rawCommand.RequiredPermissions}")}}, dmPermission: {{rawCommand.AllowPrivateUsage.ToString().ToLower()}}, isNsfw: {{rawCommand.IsNsfw.ToString().ToLower()}})]
                                    public sealed class {{GetUniqueCodeCompatibleName()}} : {{nameof(ApplicationCommandsModule)}}
                                    {
                                        public {{nameof(Bot)}} _bot { private get; set; }

                                        // EntryPoint
                                    }
                                }
                                """;

                                foreach (var rawSubCommand in rawCommand.SubCommands)
                                {
                                    rawSubCommand.Parent = rawCommand;

                                    var IndexPath = $"// EntryPoint";
                                    int InsertPosition = InsertPosition = code.IndexOf(IndexPath) + IndexPath.Length;

                                    code = code.Insert(InsertPosition, GetGroupMethodCode(plugin, nameof(InteractionContext), rawSubCommand, rawCommand));
                                }

                                compilationList.Add(CSharpCompilation.Create(GetUniqueCodeCompatibleName())
                                    .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(code))
                                    .AddReferences(references)
                                    .WithOptions(options),
                                        new CompilationData("app_group", code, rawCommand));
                            }
                            else
                            {
                                var code =
                                $$"""
                                {{classUsings}}

                                public sealed class {{GetUniqueCodeCompatibleName()}} : {{nameof(ApplicationCommandsModule)}}
                                {
                                    public {{nameof(Bot)}} _bot { private get; set; }

                                    {{GetSingleMethodCode(plugin, nameof(InteractionContext), rawCommand)}}
                                }
                                """;

                                compilationList.Add(CSharpCompilation.Create(GetUniqueCodeCompatibleName())
                                    .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(code))
                                    .AddReferences(references)
                                    .WithOptions(options),
                                        new CompilationData("app_single", code, rawCommand));
                            }
                    }

                    foreach (var compilation in compilationList)
                    {
                        try
                        {
                            using (var stream = new MemoryStream())
                            {
                                EmitResult result = compilation.Key.Emit(stream);
                                if (!result.Success)
                                {
                                    _logger.LogError("Failed to emit compilation\n{diagnostics}",
                                        JsonConvert.SerializeObject(result.Diagnostics.Select(x => $"{x.Id}: {x.Location}: {compilation.Value.code[x.Location.SourceSpan.Start..x.Location.SourceSpan.End]}"), Formatting.Indented));

                                    Exception exception = new();
                                    exception.Data.Add("diagnostics", result.Diagnostics);
                                    throw exception;
                                }

                                byte[] assemblyBytes = stream.ToArray();
                                Assembly assembly = Assembly.Load(assemblyBytes);
                                assemblyList.Add(assembly, compilation.Value.type);

                                Directory.CreateDirectory("CompiledPluginCommands");

                                var path = $"CompiledPluginCommands/{assembly.GetName().Name}.dll";
                                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                                {
                                    stream.Seek(0, SeekOrigin.Begin);
                                    await stream.CopyToAsync(fileStream);
                                    await fileStream.FlushAsync();

                                    pluginInfo.CompiledCommands.Add(path, compilation.Value.type);
                                }

                                _logger.LogDebug("Compiled class for command '{command}' of type '{type}'", compilation.Value.command.Name, compilation.Value.type);
                                _logger.LogTrace($"\n{compilation.Value.code}");
                            }
                        }
                        catch (Exception ex)
                        {
                            var diagnostics = (ImmutableArray<Diagnostic>)ex.Data["diagnostics"];

                            _logger.LogError("Failed Compilation of class type '{type}'", compilation.Value.type);
                            _logger.LogTrace($"\n{compilation.Value.code}");

                            await Task.Delay(1000);

                            Console.WriteLine();
                            for (int i = 0; i < compilation.Value.code.Length; i++)
                            {
                                Diagnostic? foundDiagnostic = diagnostics.FirstOrDefault(x => i >= x.Location.SourceSpan.Start && i <= x.Location.SourceSpan.End, null);

                                if (foundDiagnostic is not null)
                                    switch (foundDiagnostic.Severity)
                                    {
                                        case DiagnosticSeverity.Hidden:
                                            Console.ForegroundColor = ConsoleColor.Gray;
                                            break;
                                        case DiagnosticSeverity.Info:
                                            Console.ForegroundColor = ConsoleColor.Cyan;
                                            break;
                                        case DiagnosticSeverity.Warning:
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            break;
                                        case DiagnosticSeverity.Error:
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            break;
                                        default:
                                            break;
                                    }
                                else
                                    Console.ForegroundColor = ConsoleColor.White;

                                Console.Write(compilation.Value.code[i]);
                            }
                            Console.WriteLine();
                        }
                    }

                    RegisterAssemblies(_bot, cNext, appCommands, plugin.Value, assemblyList);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load commands", ex);
                _logger.LogError("Affected plugin: {0}", plugin.Value.Name);
            }
        }

        _bot.status.LoadedConfig.DontModify.LastKnownHash = applicationHash;
        _bot.status.LoadedConfig.Save();
    }

    private static void RegisterAssemblies(Bot _bot, CommandsNextExtension cNext, ApplicationCommandsExtension appCommands, BasePlugin plugin, Dictionary<Assembly, string> assemblyList)
    {
        foreach (var assembly in assemblyList)
        {
            foreach (var parentType in assembly.Key.GetTypes())
            {
                foreach (var method in parentType.GetMethods())
                {
                    if (method.Name.StartsWith("Populate"))
                        method.Invoke(null, new object[] { _bot });
                }

                foreach (var subTypes in parentType.GetNestedTypes())
                {
                    foreach (var method in parentType.GetMethods())
                    {
                        if (method.Name.StartsWith("Populate"))
                            method.Invoke(null, new object[] { _bot });
                    }
                }
            }
        }

        foreach (var assembly in assemblyList)
        {
            switch (assembly.Value)
            {
                case "prefix_single":
                case "prefix_group":
                    cNext.RegisterCommands(assembly.Key.GetTypes().First(x => x.BaseType == typeof(BaseCommandModule)));
                    break;

                case "app_single":
                case "app_group":
                    if (_bot.status.LoadedConfig.IsDev)
                        appCommands.RegisterGuildCommands(assembly.Key.GetTypes().First(x => x.BaseType == typeof(ApplicationCommandsModule)), _bot.status.LoadedConfig.Channels.Assets, plugin.EnableCommandTranslations);
                    else
                        appCommands.RegisterGlobalCommands(assembly.Key.GetTypes().First(x => x.BaseType == typeof(ApplicationCommandsModule)), plugin.EnableCommandTranslations);
                    break;
            }
        }
    }

    private static string GetUsings()
    {
        if (CachedUsings.IsNullOrWhiteSpace())
            CachedUsings = """
            namespace ProjectMakoto;

            """ + string.Join("\n", File.ReadAllText("Global.cs").ReplaceLineEndings("\n").Split("\n").Where(x => !x.StartsWith("//"))).Replace("global ", "");

        return CachedUsings;
    }

    private static string GetUniqueCodeCompatibleName()
        => $"a{Guid.NewGuid().ToString().ToLower().Replace("-", "")}";

    private static string GetSingleMethodCode(KeyValuePair<string, BasePlugin> PluginIdentifier, string ContextName, BasePluginCommand Command)
    {
        var TaskName = GetUniqueCodeCompatibleName();

        string GetMethodLine()
        {
            if (ContextName == nameof(InteractionContext))
                return
                    $$"""
                    [SlashCommand("{{Command.Name}}", "{{Command.Description}}"{{(Command.RequiredPermissions is null ? "" : $", {(long)Command.RequiredPermissions}")}}, dmPermission: {{Command.AllowPrivateUsage.ToString().ToLower()}}, isNsfw: {{Command.IsNsfw.ToString().ToLower()}})]
                    public Task {{TaskName}}_Execute({{ContextName}} ctx{{(Command.Overloads.Length > 0 ? ", " : "")}}{{string.Join(", ", Command.Overloads.Select(x => $"[Option(\"{x.Name}\", \"{x.Description}\")] {x.Type.Name} {x.Name} {(x.Required ? "" : " = null")}"))}})
                    """;
            else if (ContextName == nameof(CommandContext))
                return
                    $$"""
                    [Command("{{Command.Name}}"), CommandModule("{{Command.Module}}"), Description("{{Command.Description}}")]
                    public Task a{{TaskName}}_Execute(CommandContext ctx{{(Command.Overloads.Length > 0 ? ", " : "")}}{{string.Join(", ", Command.Overloads.Select(x => $"{(x.UseRemainingString ? "[RemainingText]" : "")} [Description(\"{x.Description}\")] {x.Type.Name} {x.Name} {(x.Required ? "" : " = null")}"))}})
                    """;
            else
                throw new NotImplementedException();
        }
        return $$"""

            {{GetMethodLine()}}
            {
                try
                {
                    Task t = (Task){{TaskName}}_CommandMethod.Invoke(Activator.CreateInstance({{TaskName}}_CommandType),
                        new object[] 
                        { ctx, _bot, new Dictionary<string, object>
                            {
                                {{string.Join(",\n", Command.Overloads.Select(x => $"{{ \"{x.Name}\", {x.Name} }}"))}}
                            }{{(ContextName == nameof(InteractionContext) ? ", true, true, false" : "")}}
                        });

                    t.Add(_bot.watcher, ctx);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to execute plugin's application command", ex);
                }

                return Task.CompletedTask;
            }

            private static Type {{TaskName}}_CommandType { get; set; }
            private static MethodInfo {{TaskName}}_CommandMethod { get; set; }
            public static void Populate_{{TaskName}}({{nameof(Bot)}} _bot)
            {
                _logger.LogDebug("Populating execution properties for '{CommandName}':'{taskname}'", "{{Command.Name}}","{{TaskName}}");
                {{TaskName}}_CommandType = _bot.PluginCommands["{{PluginIdentifier.Key}}"].First(x => x.Name == "{{Command.Name}}").Command.GetType();
                {{TaskName}}_CommandMethod = {{TaskName}}_CommandType.GetMethods().First(x => x.Name == "ExecuteCommand" && x.GetParameters().Any(param => param.ParameterType == typeof({{ContextName}})));
            }
            """;
    }
    
    private static string GetGroupMethodCode(KeyValuePair<string, BasePlugin> PluginIdentifier, string ContextName, BasePluginCommand Command, BasePluginCommand Parent)
    {
        var TaskName = GetUniqueCodeCompatibleName();

        string GetMethodLine()
        {
            if (ContextName == nameof(InteractionContext))
                return
                    $$"""
                    [SlashCommand("{{Command.Name}}", "{{Command.Description}}")]
                    public Task {{TaskName}}_Execute({{ContextName}} ctx{{(Command.Overloads.Length > 0 ? ", " : "")}}{{string.Join(", ", Command.Overloads.Select(x => $"[Option(\"{x.Name}\", \"{x.Description}\")] {x.Type.Name} {x.Name} {(x.Required ? "" : " = null")}"))}})
                    """;
            else if (ContextName == nameof(CommandContext))
                return
                    $$"""
                    [Command("{{Command.Name}}"), CommandModule("{{Command.Module}}"), Description("{{Command.Description}}")]
                    public Task a{{TaskName}}_Execute(CommandContext ctx{{(Command.Overloads.Length > 0 ? ", " : "")}}{{string.Join(", ", Command.Overloads.Select(x => $"{(x.UseRemainingString ? "[RemainingText]" : "")} [Description(\"{x.Description}\")] {x.Type.Name} {x.Name} {(x.Required ? "" : " = null")}"))}})
                    """;
            else
                throw new NotImplementedException();
        }
        return $$"""

            {{GetMethodLine()}}
            {
                try
                {
                    Task t = (Task){{TaskName}}_CommandMethod.Invoke(Activator.CreateInstance({{TaskName}}_CommandType), 
                            new object[] 
                            { ctx, _bot, new Dictionary<string, object>
                                {
                                    {{string.Join(",\n", Command.Overloads.Select(x => $"{{ \"{x.Name}\", {x.Name} }}"))}}
                                }{{(ContextName == nameof(InteractionContext) ? ", true, true, false" : "")}}
                            });
                    
                    TaskWatcherExtensions.Add(t, _bot.watcher, ctx);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to execute plugin's prefix command", ex);
                }
                    
                return Task.CompletedTask;
            }

            private static Type {{TaskName}}_CommandType { get; set; }
            private static MethodInfo {{TaskName}}_CommandMethod { get; set; }
            public static void Populate_{{TaskName}}({{nameof(Bot)}} _bot)
            {
                _logger.LogDebug("Populating execution properties for '{ParentCommandName} {CommandName}':'{taskname}'", "{{Parent.Name}}", "{{Command.Name}}","{{TaskName}}");
                {{TaskName}}_CommandType = _bot.PluginCommands["{{PluginIdentifier.Key}}"].First(x => x.Name == "{{Parent.Name}}").SubCommands.First(x => x.Name == "{{Command.Name}}").Command.GetType();
                {{TaskName}}_CommandMethod = {{TaskName}}_CommandType.GetMethods().First(x => x.Name == "ExecuteCommand" && x.GetParameters().Any(param => param.ParameterType == typeof({{ContextName}})));
            }
            """;
    }
}

internal record CompilationData(string type, string code, BasePluginCommand command);