// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.Immutable;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ProjectMakoto.Util.Initializers;
internal static class PluginLoader
{
    private static string CachedUsings = "";

    internal static async Task LoadPlugins(Bot bot)
    {
        if (!bot.status.LoadedConfig.EnablePlugins)
            return;

        _logger.LogDebug("Loading Plugins..");
        foreach (var pluginFile in Directory.GetFiles("Plugins").Where(x => x.EndsWith(".pmpl")))
        {
            if (new DirectoryInfo(pluginFile).Name.StartsWith('.'))
                continue;

            var pluginName = Path.GetFileName(pluginFile);

            _logger.LogDebug("Loading Plugin '{Name}'..", pluginName);

            using var pluginFileStream = new FileStream(pluginFile, FileMode.Open, FileAccess.ReadWrite);
            using var zipArchive = new ZipArchive(pluginFileStream, ZipArchiveMode.Update);

            var referenceFiles = zipArchive.Entries;
            Assembly? resolveAssemblyEvent(object? obj, ResolveEventArgs arg)
            {
                var name = $"{new AssemblyName(arg.Name).Name}.dll";
                var assemblyFile = referenceFiles.Where(x => x.Name.EndsWith(name)).FirstOrDefault();
                if (assemblyFile != null)
                {
                    using var assemblyStream = assemblyFile.Open();
                    return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
                }

                throw new Exception($"Could not locate: '{name}' ({arg.RequestingAssembly?.FullName})");
            }

            AppDomain.CurrentDomain.AssemblyResolve += resolveAssemblyEvent;

            try
            {
                var count = 0;

                foreach (var assemblyEntry in referenceFiles.Where(x => x.Name.EndsWith(".dll")))
                {
                    AssemblyLoadContext pluginLoadContext = new(null);

                    var assemblyName = Path.GetFileNameWithoutExtension(assemblyEntry.Name);

                    if (AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName()).Any(x => x.Name == assemblyName))
                    {
                        _logger.LogTrace("{Assembly} already loaded, skipping", assemblyName);
                        continue;
                    }

                    using var assemblyStream = assemblyEntry.Open();
                    var assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream);

                    try
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            if (typeof(BasePlugin).IsAssignableFrom(type))
                            {
                                _logger.LogDebug("Loading Plugin from '{0}'", assemblyEntry);

                                count++;
                                var result = Activator.CreateInstance(type) as BasePlugin;
                                result.LoadedFile = new FileInfo(pluginFile);

                                if (result.SupportedPluginApis == null || !result.SupportedPluginApis.Contains(BasePlugin.CurrentApiVersion))
                                    throw new IndexOutOfRangeException($"Plugin does not support Api Version {BasePlugin.CurrentApiVersion}");

                                bot._Plugins.Add(assemblyName, result);

                                UniversalExtensions.LoadAllReferencedAssemblies(assembly.GetReferencedAssemblies());
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = bot._Plugins.Remove(assemblyName);
                        _logger.LogError("Failed to load Plugin '{0}' from '{1}'", ex, assemblyName, assemblyEntry);
                    }
                }

                if (count == 0)
                {
                    _logger.LogWarn("Cannot load Plugin '{0}': Plugin Assembly does not contain type that inherits BasePlugin.", pluginName);
                    continue;
                }

                _logger.LogInfo("Loaded Plugin from '{0}'", pluginName);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolveAssemblyEvent;
            }
        }

        _logger.LogInfo("Loaded {0} Plugins.", bot.Plugins.Count);

        foreach (var b in bot.Plugins)
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

            if (b.Value.Version is null)
            {
                _logger.LogWarn("Skipped loading Plugin '{0}': Missing Version.", b.Key);
                continue;
            }

            _logger.LogDebug("Initializing Plugin '{0}' ({1})..", b.Value.Name, b.Key);

            try
            {
                b.Value.Load(bot);
                _logger.LogInfo("Initialized Plugin from '{0}': '{1}' (v{2}).", b.Key, b.Value.Name, b.Value.Version.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize Plugin from '{0}': '{1}' (v{2}).", ex, b.Key, b.Value.Name, b.Value.Version.ToString());
            }

            try
            {
                await b.Value.CheckForUpdates();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to check updates for '{PluginName}'", ex, b.Value.Name);
            }

            try
            {
                var (path, type) = b.Value.LoadTranslations();

                if (path != null)
                {
                    using var stream = b.Value.LoadedFile.Open(FileMode.Open);
                    using var zip = new ZipArchive(stream);
                    using var file = zip.GetEntry(path).Open();
                    using var reader = new StreamReader(file);

                    b.Value.UsesTranslations = true;
                    b.Value.Translations = (ITranslations)JsonConvert.DeserializeObject(reader.ReadToEnd(), type);

                    foreach (var item in b.Value.Translations.CommandList)
                        bot.LoadedTranslations.CommandList = bot.LoadedTranslations.CommandList.Add(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load translations for '{PluginName}'", ex, b.Value.Name);
            }
        }
    }

    internal static async Task LoadPluginCommands(Bot bot, IReadOnlyDictionary<int, CommandsNextExtension> cNext, IReadOnlyDictionary<int, ApplicationCommandsExtension> appCommands)
    {
        var applicationHash = HashingExtensions.ComputeSHA256Hash(new FileInfo(Assembly.GetExecutingAssembly().Location));

        foreach (var plugin in bot.Plugins)
        {
            try
            {
                var pluginHash = HashingExtensions.ComputeSHA256Hash(plugin.Value.LoadedFile);
                Dictionary<Assembly, string> assemblyList = new();

                var pluginCommands = await plugin.Value.RegisterCommands();
                bot._PluginCommands.Add(plugin.Key, pluginCommands.ToList());

                if (bot.status.LoadedConfig.PluginCache.TryGetValue(plugin.Key, out var pluginInfo) &&
                    pluginHash == pluginInfo.LastKnownHash &&
                    applicationHash == bot.status.LoadedConfig.DontModify.LastKnownHash &&
                    pluginInfo.CompiledCommands.All(x => File.Exists(x.Key)) &&
                    pluginInfo.CompiledCommands.Count != 0)
                {
                    _logger.LogInfo("Loading {0} Commands from Plugin from '{1}' ({2}) from compiled assemblies..", pluginInfo.CompiledCommands.Count, plugin.Value.Name, plugin.Value.Version.ToString());

                    foreach (var b in pluginInfo.CompiledCommands)
                    {
                        AssemblyLoadContext pluginLoadContext = new(null);
                        using var file = new FileStream(b.Key, FileMode.Open, FileAccess.Read);
                        var assembly = pluginLoadContext.LoadFromStream(file);

                        assemblyList.Add(assembly, b.Value);
                    }

                    RegisterAssemblies(bot, cNext, appCommands, plugin.Value, assemblyList);
                    continue;
                }

                _ = bot.status.LoadedConfig.PluginCache.TryAdd(plugin.Key, new());
                pluginInfo = bot.status.LoadedConfig.PluginCache[plugin.Key];

                pluginInfo.LastKnownHash = pluginHash;

                if (pluginInfo.CompiledCommands.Count != 0)
                    _ = FileExtensions.CleanupFilesAndDirectories(new(), pluginInfo.CompiledCommands.Select(x => x.Key).ToList());

                pluginInfo.CompiledCommands = new();

                var classUsings = GetUsings();

                if (pluginCommands.IsNotNullAndNotEmpty())
                {
                    var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                        .WithOptimizationLevel(OptimizationLevel.Release)
                        .WithDeterministic(true);
                    
                    var references = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(x => !x.IsDynamic && !x.Location.IsNullOrWhiteSpace())
                        .Select(x => MetadataReference.CreateFromFile(x.Location));

                    _logger.LogInfo("Compiling {0} BasePluginCommands from Plugin from '{1}' ({2}).", pluginCommands.Count(), plugin.Value.Name, plugin.Value.Version.ToString());

                    var compilationList = new Dictionary<CSharpCompilation, CompilationData>();

                    Dictionary<string, string> getClassCode(BasePluginCommand command)
                    {
                        var rawCodeList = new Dictionary<string, string>();

                        foreach (var supportedType in command.SupportedCommandTypes)
                        {
                            string createCodeWithDefaultClass(string code)
                            {
                                var inheritType = supportedType switch
                                {
                                    PluginCommandType.SlashCommand or PluginCommandType.ContextMenu => typeof(ApplicationCommandsModule),
                                    PluginCommandType.PrefixCommand => typeof(BaseCommandModule),
                                    _ => throw new NotImplementedException()
                                };

                                return $$"""
                                    {{classUsings}}

                                    public sealed class {{GetUniqueCodeCompatibleName()}} : {{inheritType.FullName}}
                                    {
                                        {{code}}
                                    }
                                    """;
                            }

                            string getAttribute(BasePluginCommand command)
                            {
                                switch (supportedType)
                                {
                                    case PluginCommandType.SlashCommand:
                                        if (command.IsGroup)
                                            return $$"""
                                                [{{typeof(SlashCommandGroupAttribute).FullName}}("{{command.Name}}", "{{command.Description}}"{{(command.RequiredPermissions is null ? "" : $", {(long)command.RequiredPermissions}")}}, dmPermission: {{command.AllowPrivateUsage.ToString().ToLower()}}, isNsfw: {{command.IsNsfw.ToString().ToLower()}})]
                                                """;
                                        else
                                            return $$"""
                                                [{{typeof(SlashCommandAttribute).FullName}}("{{command.Name}}", "{{command.Description}}"{{(command.RequiredPermissions is null ? "" : $", {(long)command.RequiredPermissions}")}}, dmPermission: {{command.AllowPrivateUsage.ToString().ToLower()}}, isNsfw: {{command.IsNsfw.ToString().ToLower()}})]
                                                """;
                                    case PluginCommandType.PrefixCommand:
                                        if (command.IsGroup)
                                            return $$"""
                                                [{{typeof(GroupAttribute).FullName}}("{{command.Name}}"), {{typeof(DescriptionAttribute).FullName}}("{{command.Description}}")]
                                                """;
                                        else
                                            return $$"""
                                                [{{typeof(CommandAttribute).FullName}}("{{command.AlternativeName ?? command.Name}}"), {{typeof(DescriptionAttribute).FullName}}("{{command.Description}}")]
                                                """;
                                    case PluginCommandType.ContextMenu:
                                        return $$"""
                                                [{{typeof(ContextMenuAttribute).FullName}}({{typeof(ApplicationCommandType).FullName}}.{{Enum.GetName(typeof(ApplicationCommandType), command.ContextMenuType)}}, "{{command.Name}}", dmPermission: {{command.AllowPrivateUsage.ToString().ToLower()}}, isNsfw: {{command.IsNsfw.ToString().ToLower()}})]
                                                """;

                                    default:
                                        throw new NotImplementedException();
                                }
                            }

                            string getMethodDefinition(BasePluginCommand command, BasePluginCommand? parent)
                            {
                                var TaskName = GetUniqueCodeCompatibleName();

                                string getPopulationMethods()
                                {
                                    var contextType = supportedType switch
                                    {
                                        PluginCommandType.SlashCommand => typeof(InteractionContext),
                                        PluginCommandType.PrefixCommand => typeof(CommandContext),
                                        PluginCommandType.ContextMenu => typeof(ContextMenuContext),
                                        _ => throw new NotImplementedException(),
                                    };

                                    return $$"""
                                        private static {{typeof(Type).FullName}} {{TaskName}}_CommandType { get; set; }
                                        private static {{typeof(MethodInfo).FullName}} {{TaskName}}_CommandMethod { get; set; }
                                        public static void Populate_{{TaskName}}({{typeof(Bot).FullName}} _bot)
                                        {
                                            _logger.LogDebug("Populating execution properties for '{CommandName}':'{taskname}'", "{{command.Name}}","{{TaskName}}");
                                            {{(parent is null ? $"{TaskName}_CommandType = _bot.PluginCommands[\"{plugin.Key}\"].First(x => x.Name == \"{command.Name}\").Command.GetType();" : $"{TaskName}_CommandType = _bot.PluginCommands[\"{plugin.Key}\"].First(x => x.Name == \"{parent.Name}\").SubCommands.First(x => x.Name == \"{command.Name}\").Command.GetType();")}}
                                            {{TaskName}}_CommandMethod = {{TaskName}}_CommandType.GetMethods().First(x => x.Name == "ExecuteCommand" && x.GetParameters().Any(param => param.ParameterType == typeof({{contextType.FullName}})));
                                        }
                                        """;
                                }

                                switch (supportedType)
                                {
                                    case PluginCommandType.SlashCommand:
                                        if (command.IsGroup)
                                            return $$"""
                                                {{getAttribute(command)}}
                                                public sealed class {{GetUniqueCodeCompatibleName()}} : {{typeof(ApplicationCommandsModule).FullName}}
                                                {
                                                    public {{typeof(Bot).FullName}} _bot { private get; set; }
                                                
                                                    {{string.Join("\n\n", command.SubCommands.Select(x => $$"""
                                                        {{getMethodDefinition(x, command)}}
                                                    """))}}
                                                }
                                                """;
                                        else
                                            return $$"""
                                                {{getAttribute(command)}}
                                                public {{typeof(Task).FullName}} {{TaskName}}_Execute({{typeof(InteractionContext).FullName}} ctx{{(command.Overloads?.Length > 0 ? ", " : "")}}{{string.Join(", ", command.Overloads?.Select(x => $"[{typeof(OptionAttribute).FullName}(\"{x.Name}\", \"{x.Description}\")] {x.Type.Name} {x.Name} {(x.Required ? "" : " = null")}"))}})
                                                {
                                                    try
                                                    {
                                                        {{typeof(Task).FullName}} t = ({{typeof(Task).FullName}}){{TaskName}}_CommandMethod.Invoke({{typeof(Activator).FullName}}.CreateInstance({{TaskName}}_CommandType),
                                                            new {{typeof(object[]).FullName}} 
                                                            { ctx, _bot, new Dictionary<string, object>
                                                                {
                                                                    {{string.Join(",\n", command.Overloads?.Select(x => $"{{ \"{x.Name}\", {x.Name} }}"))}}
                                                                }, {{command.IsEphemeral.ToString().ToLower()}}, true, false
                                                            });
                                                
                                                        t.Add(_bot, ctx);
                                                    }
                                                    catch ({{typeof(Exception).FullName}} ex)
                                                    {
                                                        _logger.LogError($"Failed to execute plugin's application command", ex);
                                                    }
                                                
                                                    return {{typeof(Task).FullName}}.CompletedTask;
                                                }
                                                
                                                {{getPopulationMethods()}}
                                                """;
                                    case PluginCommandType.PrefixCommand:
                                        if (command.IsGroup)
                                            return $$"""
                                                {{getAttribute(command)}}
                                                public sealed class {{GetUniqueCodeCompatibleName()}} : {{typeof(BaseCommandModule).FullName}}
                                                {
                                                    public {{typeof(Bot).FullName}} _bot { private get; set; }
                                                
                                                    {{(command.UseDefaultHelp ? $$"""
                                                    
                                                    [{{typeof(GroupCommandAttribute).FullName}}, {{typeof(CommandAttribute).FullName}}("help"), {{typeof(DescriptionAttribute).FullName}}("Sends a list of available sub-commands")]
                                                    public async {{typeof(Task).FullName}} Help({{typeof(CommandContext).FullName}} ctx)
                                                        => {{typeof(PrefixCommandUtil).FullName}}.SendGroupHelp(_bot, ctx, "{{command.Name}}").Add(_bot, ctx);
                                                    """ : "")}}

                                                    {{string.Join("\n\n", command.SubCommands.Select(x => $$"""
                                                        {{getMethodDefinition(x, command)}}
                                                    """))}}
                                                }
                                                """;
                                        else
                                            return $$"""
                                                {{getAttribute(command)}}
                                                public {{typeof(Task).FullName}} {{TaskName}}_Execute({{typeof(CommandContext).FullName}} ctx{{(command.Overloads?.Length > 0 ? ", " : "")}}{{string.Join(", ", command.Overloads?.Select(x => $"{(x.UseRemainingString ? $"[{typeof(RemainingTextAttribute).FullName}]" : "")} [{typeof(DescriptionAttribute).FullName}(\"{x.Description}\")] {x.Type.Name} {x.Name} {(x.Required ? "" : " = null")}") ?? [])}})
                                                {
                                                    try
                                                    {
                                                        {{typeof(Task).FullName}} t = ({{typeof(Task).FullName}}){{TaskName}}_CommandMethod.Invoke({{typeof(Activator).FullName}}.CreateInstance({{TaskName}}_CommandType),
                                                        new {{typeof(object[]).FullName}} 
                                                        { ctx, _bot, new Dictionary<string, object>
                                                            {
                                                                {{string.Join(",\n", command.Overloads?.Select(x => $"{{ \"{x.Name}\", {x.Name} }}") ?? [])}}
                                                            }
                                                        });

                                                        t.Add(_bot, ctx);
                                                    }
                                                    catch ({{typeof(Exception).FullName}} ex)
                                                    {
                                                        _logger.LogError($"Failed to execute plugin's command", ex);
                                                    }

                                                    return {{typeof(Task).FullName}}.CompletedTask;
                                                }

                                                {{getPopulationMethods()}}
                                                """;
                                    case PluginCommandType.ContextMenu:
                                        return $$"""
                                                {{(!command.AlternativeName.IsNullOrWhiteSpace() ? $"[{typeof(PrefixCommandAlternativeAttribute).FullName}(\"{command.AlternativeName}\")]" : "")}}
                                                {{getAttribute(command)}}
                                                public {{typeof(Task).FullName}} {{TaskName}}_Execute({{typeof(ContextMenuContext).FullName}} ctx)
                                                {
                                                        try
                                                    {
                                                        {{typeof(Task).FullName}} t = ({{typeof(Task).FullName}}){{TaskName}}_CommandMethod.Invoke({{typeof(Activator).FullName}}.CreateInstance({{TaskName}}_CommandType),
                                                        new {{typeof(object[]).FullName}} 
                                                        { ctx, _bot, new Dictionary<string, object>
                                                            {
                                                                {{(command.ContextMenuType == ApplicationCommandType.Message ? "{ \"message\", ctx.TargetMessage }" : "{ \"user\", ctx.TargetMember ?? ctx.TargetUser }")}}
                                                            }, {{command.IsEphemeral.ToString().ToLower()}}, true, false
                                                        });
                                                
                                                        t.Add(_bot, ctx);
                                                    }
                                                    catch ({{typeof(Exception).FullName}} ex)
                                                    {
                                                        _logger.LogError($"Failed to execute plugin's command", ex);
                                                    }
                                                
                                                    return {{typeof(Task).FullName}}.CompletedTask;
                                                }

                                                {{getPopulationMethods()}}
                                                """;

                                    default:
                                        throw new NotImplementedException();
                                }
                            }

                            var currentCode = $$"""
                                public {{typeof(Bot).FullName}} _bot { private get; set; }

                                {{getMethodDefinition(command, null)}}
                                """;

                            string getDefinition(PluginCommandType type) 
                                => type switch
                                {
                                    PluginCommandType.ContextMenu or PluginCommandType.SlashCommand => "app",
                                    PluginCommandType.PrefixCommand => "prefix",
                                    _ => throw new NotImplementedException(),
                                };

                            var value = createCodeWithDefaultClass(currentCode);
                            rawCodeList.Add($"{getDefinition(supportedType)}_{(command.IsGroup ? "group" : "single")}", value);
                        }

                        return rawCodeList;
                    }

                    foreach (var rawCommand in pluginCommands)
                    {
                        rawCommand.Registered = true;
                        foreach (var b in getClassCode(rawCommand))
                        {
                            compilationList.Add(CSharpCompilation.Create(GetUniqueCodeCompatibleName())
                                    .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(b.Value))
                                    .AddReferences(references)
                                    .WithOptions(options),
                                        new CompilationData(b.Key, b.Value, rawCommand));
                        }
                    }

                    foreach (var compilation in compilationList)
                    {
                        try
                        {
                            using (var stream = new MemoryStream())
                            {
                                var result = compilation.Key.Emit(stream);
                                if (!result.Success)
                                {
                                    _logger.LogError("Failed to emit compilation\n{diagnostics}",
                                        JsonConvert.SerializeObject(result.Diagnostics.Select(x => $"{x.Id}: {x.Location}: {compilation.Value.code[x.Location.SourceSpan.Start..x.Location.SourceSpan.End]}"), Formatting.Indented));

                                    Exception exception = new();
                                    exception.Data.Add("diagnostics", result.Diagnostics);
                                    throw exception;
                                }

                                var assemblyBytes = stream.ToArray();
                                var assembly = Assembly.Load(assemblyBytes);
                                assemblyList.Add(assembly, compilation.Value.type);

                                _ = Directory.CreateDirectory("CompiledPluginCommands");

                                var path = $"CompiledPluginCommands/{assembly.GetName().Name}.dll";
                                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                                {
                                    _ = stream.Seek(0, SeekOrigin.Begin);
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
                            for (var i = 0; i < compilation.Value.code.Length; i++)
                            {
                                var foundDiagnostic = diagnostics.FirstOrDefault(x => i >= x.Location.SourceSpan.Start && i <= x.Location.SourceSpan.End, null);

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

                            _ = Console.ReadLine();
                        }
                    }

                    RegisterAssemblies(bot, cNext, appCommands, plugin.Value, assemblyList);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load commands", ex);
                _logger.LogError("Affected plugin: {0}", plugin.Value.Name);
            }
        }

        bot.status.LoadedConfig.DontModify.LastKnownHash = applicationHash;
        bot.status.LoadedConfig.Save();
    }

    private static void RegisterAssemblies(Bot _bot, IReadOnlyDictionary<int, CommandsNextExtension> cNext, IReadOnlyDictionary<int, ApplicationCommandsExtension> appCommands, BasePlugin plugin, Dictionary<Assembly, string> assemblyList)
    {
        foreach (var assembly in assemblyList)
        {
            foreach (var parentType in assembly.Key.GetTypes())
            {
                foreach (var method in parentType.GetMethods())
                {
                    if (method.Name.StartsWith("Populate"))
                        _ = method.Invoke(null, new object[] { _bot });
                }

                foreach (var subType in parentType.GetNestedTypes())
                {
                    foreach (var method in subType.GetMethods())
                    {
                        if (method.Name.StartsWith("Populate"))
                            _ = method.Invoke(null, new object[] { _bot });
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
                        appCommands.RegisterGuildCommands(assembly.Key.GetTypes().First(x => x.BaseType == typeof(ApplicationCommandsModule)), _bot.status.LoadedConfig.Discord.DevelopmentGuild, plugin.EnableCommandTranslations);
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
}

internal record CompilationData(string type, string code, BasePluginCommand command);