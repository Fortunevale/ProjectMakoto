// Project Makoto
// Copyright (C) 2024  Fortunevale
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
internal static class CommandCompiler
{
    internal static async Task<(Assembly compiledCommands, CompilationType Type)[]> BuildCommands(Bot bot, string applicationHash, IEnumerable<MakotoModule> moduleList, KeyValuePair<string, BasePlugin>? plugin = null)
    {
        var isPlugin = plugin != null;
        plugin ??= new KeyValuePair<string, BasePlugin>("Built-In", null);
        List<(Assembly compiledCommands, CompilationType Type)> assemblyList = new();

        string currentHash;

        if (plugin.Value.Key != "Built-In")
            currentHash = HashingExtensions.ComputeSHA256Hash(plugin.Value.Value.LoadedFile);
        else
            currentHash = applicationHash;

        if (bot.status.LoadedConfig.CommandCache.TryGetValue(plugin.Value.Key, out var supplierInfo) &&
            currentHash == supplierInfo.LastKnownHash &&
            applicationHash == bot.status.LoadedConfig.DontModify.LastKnownHash &&
            supplierInfo.CompiledCommands.All(x => File.Exists(x.Key)) &&
            supplierInfo.CompiledCommands.Count != 0)
        {
            if (isPlugin)
                Log.Information("Loading {0} Commands from Plugin from '{1}' ({2}) from compiled assemblies..",
                    supplierInfo.CompiledCommands.Count,
                    plugin.Value.Value.Name,
                    plugin.Value.Value.Version.ToString());
            else
                Log.Information("Loading {0} Commands from compiled assemblies..",
                    supplierInfo.CompiledCommands.Count);

            foreach (var b in supplierInfo.CompiledCommands)
            {
                AssemblyLoadContext loadContext = new(null);
                using var file = new FileStream(b.Key, FileMode.Open, FileAccess.Read);
                var assembly = loadContext.LoadFromStream(file);

                assemblyList.Add((assembly, b.Value));
            }

            return assemblyList.ToArray();
        }

        _ = bot.status.LoadedConfig.CommandCache.TryAdd(plugin.Value.Key, new());
        supplierInfo = bot.status.LoadedConfig.CommandCache[plugin.Value.Key];
        supplierInfo.LastKnownHash = currentHash;

        if (supplierInfo.CompiledCommands.Count != 0)
            _ = FileExtensions.CleanupFilesAndDirectories(new(), supplierInfo.CompiledCommands.Select(x => x.Key).ToList());

        supplierInfo.CompiledCommands = new();

        (string Code, CompilationType Type, string ModuleName)[][] getClassCode()
        {
            var classHeader = GetFileHeader();

            string createCodeWithDefaultClass(IEnumerable<string> code, MakotoCommandType supportedType, int? Priority)
            {
                var inheritType = supportedType switch
                {
                    MakotoCommandType.SlashCommand or MakotoCommandType.ContextMenu => typeof(ApplicationCommandsModule),
                    MakotoCommandType.PrefixCommand => typeof(BaseCommandModule),
                    _ => throw new NotImplementedException()
                };

                return $$"""
                {{classHeader}}

                {{(Priority is not null ? $"[{typeof(ModulePriorityAttribute).FullName}({Priority})]" : "")}}
                public sealed class {{GetUniqueCodeCompatibleName()}} : {{inheritType.FullName}}
                {
                    public {{typeof(Bot).FullName}} _bot { private get; set; }

                    {{string.Join("\n\n", code)}}
                }
                """;
            }

            (string Code, CompilationType Type, string ModuleName)[] getModuleDefinition(MakotoModule module)
            {
                module.Registered = true;

                var rawSlashCommandList = module.Commands
                    .Where(x => x.SupportedCommandTypes.Contains(MakotoCommandType.SlashCommand))
                    .Select(x => getMethodDefinition(x, module, null, MakotoCommandType.SlashCommand));

                var rawPrefixCommandList = module.Commands
                    .Where(x => x.SupportedCommandTypes.Contains(MakotoCommandType.PrefixCommand))
                    .Select(x => getMethodDefinition(x, module, null, MakotoCommandType.PrefixCommand));

                var rawContextCommandList = module.Commands
                    .Where(x => x.SupportedCommandTypes.Contains(MakotoCommandType.ContextMenu))
                    .Select(x => getMethodDefinition(x, module, null, MakotoCommandType.ContextMenu));

                var rawCodeList = new List<(string Code, CompilationType Type, string ModuleName)>();

                if (rawSlashCommandList.Any())
                    rawCodeList.Add((createCodeWithDefaultClass(rawSlashCommandList, MakotoCommandType.SlashCommand, module.Priority), CompilationType.App, module.Name));

                if (rawContextCommandList.Any())
                    rawCodeList.Add((createCodeWithDefaultClass(rawContextCommandList, MakotoCommandType.ContextMenu, module.Priority), CompilationType.App, module.Name));

                if (rawPrefixCommandList.Any())
                    rawCodeList.Add((createCodeWithDefaultClass(rawPrefixCommandList, MakotoCommandType.PrefixCommand, module.Priority), CompilationType.Prefix, module.Name));

                return rawCodeList.ToArray();
            }

            string getMethodDefinition(MakotoCommand command, MakotoModule module, MakotoCommand? parent, MakotoCommandType supportedType)
            {
                command.Registered = true;
                var TaskName = GetUniqueCodeCompatibleName();

                if (!command.SupportedCommandTypes.Contains(supportedType))
                    return string.Empty;

                string getAttribute()
                {
                    switch (supportedType)
                    {
                        case MakotoCommandType.SlashCommand:
                            if (command.IsGroup)
                                return $$"""
                                        [{{typeof(SlashCommandGroupAttribute).FullName}}("{{command.Name}}", "{{command.Description}}"{{(command.RequiredPermissions is null ? "" : $", {(long)command.RequiredPermissions}")}}, dmPermission: {{command.AllowPrivateUsage.ToString().ToLower()}}, isNsfw: {{command.IsNsfw.ToString().ToLower()}})]
                                        """;
                            else
                                return $$"""
                                        [{{typeof(SlashCommandAttribute).FullName}}("{{command.Name}}", "{{command.Description}}"{{(command.RequiredPermissions is null ? "" : $", {(long)command.RequiredPermissions}")}}, dmPermission: {{command.AllowPrivateUsage.ToString().ToLower()}}, isNsfw: {{command.IsNsfw.ToString().ToLower()}})]
                                        """;
                        case MakotoCommandType.PrefixCommand:
                            if (command.IsGroup)
                                return $$"""
                                        [{{typeof(GroupAttribute).FullName}}("{{command.Name}}"), {{typeof(DescriptionAttribute).FullName}}("{{command.Description}}")
                                        {{(command.Aliases.IsNotNullAndNotEmpty() ? $", {typeof(AliasesAttribute).FullName}({string.Join(", ", command.Aliases.Select(x => $"\"{x}\""))})" : "")}}]
                                        """;
                            else
                                return $$"""
                                        [{{typeof(CommandAttribute).FullName}}("{{command.AlternativeName ?? command.Name}}"), {{typeof(DescriptionAttribute).FullName}}("{{command.Description}}")
                                        {{(command.Aliases.IsNotNullAndNotEmpty() ? $", {typeof(AliasesAttribute).FullName}({string.Join(", ", command.Aliases.Select(x => $"\"{x}\""))})" : "")}}]
                                        """;
                        case MakotoCommandType.ContextMenu:
                            return $$"""
                                        [{{typeof(ContextMenuAttribute).FullName}}({{typeof(ApplicationCommandType).FullName}}.{{Enum.GetName(typeof(ApplicationCommandType), command.ContextMenuType)}}, "{{command.Name}}", dmPermission: {{command.AllowPrivateUsage.ToString().ToLower()}}, isNsfw: {{command.IsNsfw.ToString().ToLower()}})]
                                        """;

                        default:
                            throw new NotImplementedException();
                    }
                }

                string getPopulationMethods()
                {
                    var contextType = supportedType switch
                    {
                        MakotoCommandType.SlashCommand => typeof(InteractionContext),
                        MakotoCommandType.PrefixCommand => typeof(CommandContext),
                        MakotoCommandType.ContextMenu => typeof(ContextMenuContext),
                        _ => throw new NotImplementedException(),
                    };

                    return isPlugin ? $$"""
                            private static {{typeof(Type).FullName}} {{TaskName}}_CommandType { get; set; }
                            private static {{typeof(MethodInfo).FullName}} {{TaskName}}_CommandMethod { get; set; }
                            public static void Populate_{{TaskName}}({{typeof(Bot).FullName}} _bot)
                            {
                                Log.Debug("Populating execution properties for '{CommandName}':'{taskname}'", "{{command.Name}}","{{TaskName}}");
                                {{(parent is null ? $"{TaskName}_CommandType = _bot.PluginCommandModules[\"{plugin.Value.Key}\"].First(x => x.Name == \"{module.Name}\").Commands.First(x => x.Name == \"{command.Name}\").Command;" : $"{TaskName}_CommandType = _bot.PluginCommandModules[\"{plugin.Value.Key}\"].Commands.First(x => x.Name == \"{module.Name}\").First(x => x.Name == \"{parent.Name}\").SubCommands.First(x => x.Name == \"{command.Name}\").Command;")}}
                                {{TaskName}}_CommandMethod = {{TaskName}}_CommandType.GetMethods().First(x => x.Name == "ExecuteCommand" && x.GetParameters().Any(param => param.ParameterType == typeof({{contextType.FullName}})));
                            }
                            """ :
                            $$"""
                            private static {{typeof(Type).FullName}} {{TaskName}}_CommandType { get; set; }
                            private static {{typeof(MethodInfo).FullName}} {{TaskName}}_CommandMethod { get; set; }
                            public static void Populate_{{TaskName}}({{typeof(Bot).FullName}} _bot)
                            {
                                Log.Debug("Populating execution properties for '{CommandName}':'{taskname}' ({module}, {command})", "{{command.Name}}","{{TaskName}}","{{module.Name}}","{{command.Name}}");
                                {{(parent is null ? $"{TaskName}_CommandType = _bot.CommandModules.First(x => x.Name == \"{module.Name}\").Commands.First(x => x.Name == \"{command.Name}\").Command;" : $"{TaskName}_CommandType = _bot.CommandModules.First(x => x.Name == \"{module.Name}\").Commands.First(x => x.Name == \"{parent.Name}\").SubCommands.First(x => x.Name == \"{command.Name}\").Command;")}}
                                {{TaskName}}_CommandMethod = {{TaskName}}_CommandType.GetMethods().First(x => x.Name == "ExecuteCommand" && x.GetParameters().Any(param => param.ParameterType == typeof({{contextType.FullName}})));
                            }
                            """;
                }

                switch (supportedType)
                {
                    case MakotoCommandType.SlashCommand:
                        if (command.IsGroup)
                            return $$"""
                                    {{getAttribute()}}
                                    public sealed class {{GetUniqueCodeCompatibleName()}} : {{typeof(ApplicationCommandsModule).FullName}}
                                    {
                                        public {{typeof(Bot).FullName}} _bot { private get; set; }
                                                
                                        {{string.Join("\n\n", command.SubCommands.Select(x => $$"""
                                            {{getMethodDefinition(x, module, command, supportedType)}}
                                        """))}}
                                    }
                                    """;
                        else
                            return $$"""
                                    {{getAttribute()}}
                                    public {{typeof(Task).FullName}} {{TaskName}}_Execute({{typeof(InteractionContext).FullName}} ctx{{(command.Overloads?.Length > 0 ? ", " : "")}}
                                        {{string.Join(", ", command.Overloads?.Select(x => $"[{typeof(OptionAttribute).FullName}(\"{x.Name}\", \"{x.Description}\", {(x.AutoCompleteType != null).ToString().ToLower()})" +
                                            $"{(x.ChannelType is not null ? $", {typeof(ChannelTypesAttribute).FullName}(({typeof(ChannelType).FullName}){(int)x.ChannelType})" : "")}" +
                                            $"{(x.MinimumValue is not null ? $", {typeof(MinimumValueAttribute).FullName}({x.MinimumValue})" : "")}" +
                                            $"{(x.MaximumValue is not null ? $", {typeof(MaximumValueAttribute).FullName}({x.MaximumValue})" : "")}" +
                                            $"{(x.AutoCompleteType is not null ? $", {typeof(AutocompleteAttribute).FullName}(typeof({x.AutoCompleteType.FullName
                                                .Replace('+', '.')}))" : "")}" +
                                            $"] {x.Type.Name}{(x.Required ? "" : "?")} {x.Name} {(x.Required ? "" : " = null")}"))}})
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
                                            Log.Error(ex, $"Failed to execute plugin's application command");
                                        }
                                                
                                        return {{typeof(Task).FullName}}.CompletedTask;
                                    }
                                                
                                    {{getPopulationMethods()}}
                                    """;
                    case MakotoCommandType.PrefixCommand:
                        if (command.IsGroup)
                            return $$"""
                                    {{getAttribute()}}
                                    public sealed class {{GetUniqueCodeCompatibleName()}} : {{typeof(BaseCommandModule).FullName}}
                                    {
                                        public {{typeof(Bot).FullName}} _bot { private get; set; }
                                                
                                        {{(command.UseDefaultHelp ? $$"""
                                                    
                                        [{{typeof(GroupCommandAttribute).FullName}}, {{typeof(CommandAttribute).FullName}}("help"), {{typeof(DescriptionAttribute).FullName}}("Sends a list of available sub-commands")]
                                        public async {{typeof(Task).FullName}} Help({{typeof(CommandContext).FullName}} ctx)
                                            => {{typeof(PrefixCommandUtil).FullName}}.SendGroupHelp(_bot, ctx, "{{command.Name}}").Add(_bot, ctx);
                                        """ : "")}}

                                        {{string.Join("\n\n", command.SubCommands.Select(x => $$"""
                                            {{getMethodDefinition(x, module, command, supportedType)}}
                                        """))}}
                                    }
                                    """;
                        else
                            return $$"""
                                    {{getAttribute()}}
                                    public {{typeof(Task).FullName}} {{TaskName}}_Execute({{typeof(CommandContext).FullName}} ctx{{(command.Overloads?.Length > 0 ? ", " : "")}}{{string.Join(", ", command.Overloads?.Select(x => $"{(x.UseRemainingString ? $"[{typeof(RemainingTextAttribute).FullName}]" : "")} [{typeof(DescriptionAttribute).FullName}(\"{x.Description}\")] {x.Type.Name}{(x.Required ? "" : "?")} {x.Name} {(x.Required ? "" : " = null")}") ?? [])}})
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
                                            Log.Error(ex, $"Failed to execute plugin's command");
                                        }

                                        return {{typeof(Task).FullName}}.CompletedTask;
                                    }

                                    {{getPopulationMethods()}}
                                    """;
                    case MakotoCommandType.ContextMenu:
                        return $$"""
                                {{(!command.AlternativeName.IsNullOrWhiteSpace() ? $"[{typeof(PrefixCommandAlternativeAttribute).FullName}(\"{command.AlternativeName}\")]" : "")}}
                                {{getAttribute()}}
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
                                        Log.Error(ex, $"Failed to execute plugin's command");
                                    }
                                                
                                    return {{typeof(Task).FullName}}.CompletedTask;
                                }

                                {{getPopulationMethods()}}
                                """;

                    default:
                        throw new NotImplementedException();
                }
            }

            var rawModules = moduleList.Select(x => getModuleDefinition(x)).ToArray();

            return rawModules;
        }

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
#if DEBUG
            .WithOptimizationLevel(OptimizationLevel.Debug)
#else
            .WithOptimizationLevel(OptimizationLevel.Release)
#endif
            .WithDeterministic(true);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !x.Location.IsNullOrWhiteSpace())
            .Select(x => MetadataReference.CreateFromFile(x.Location));

        if (isPlugin)
            Log.Information("Compiling {0} Commands from Plugin from '{1}' ({2}).",
                moduleList.Count(),
                plugin.Value.Value.Name,
                plugin.Value.Value.Version.ToString());
        else
            Log.Information("Compiling {0} Built-In Commands..",
                moduleList.Count());


        foreach (var modules in getClassCode())
        {
            foreach (var classCode in modules)
            {
                var compilation = CSharpCompilation.Create(CommandCompiler.GetUniqueCodeCompatibleName() + $"_{Regex.Replace($"{classCode.ModuleName}_{Enum.GetName(classCode.Type)}", @"[^a-zA-Z0-9_]", "")}")
                .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(classCode.Code))
                .AddReferences(references)
                .WithOptions(options);

                var data = new CompilationData(classCode.Type, classCode.Code, moduleList, plugin.Value.Key);

                try
                {
                    using (var stream = new MemoryStream())
                    {
                        var result = compilation.Emit(stream);
                        if (!result.Success)
                        {
                            Log.Error("Failed to emit compilation\n{diagnostics}",
                                JsonConvert.SerializeObject(result.Diagnostics.Select(x => $"{x.Id}: {x.GetMessage()}: {x.Location}: {data.code[x.Location.SourceSpan.Start..x.Location.SourceSpan.End]}"), Formatting.Indented));

                            Exception exception = new();
                            exception.Data.Add("diagnostics", result.Diagnostics);
                            throw exception;
                        }

                        var assemblyBytes = stream.ToArray();
                        var assembly = Assembly.Load(assemblyBytes);
                        assemblyList.Add((assembly, data.type));

                        _ = Directory.CreateDirectory("CompiledCommands");

                        var path = $"CompiledCommands/{assembly.GetName().Name}.dll";
                        using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                        {
                            _ = stream.Seek(0, SeekOrigin.Begin);
                            await stream.CopyToAsync(fileStream);
                            await fileStream.FlushAsync();

                            supplierInfo.CompiledCommands.Add(path, data.type);
                        }

#if DEBUG
                        File.WriteAllText($"CompiledCommands/{assembly.GetName().Name}.cs", classCode.Code);
#endif

                        Log.Debug("Compiled class with {cmdCount} commands for '{plugin}' of type '{type}'", data.moduleList.Sum(x => x.Commands.Count()), data.Identifier, data.type);
                        Log.Verbose($"\n{data.code}");
                    }
                }
                catch (Exception ex)
                {
                    ImmutableArray<Diagnostic>? diagnostics = null;

                    try
                    {
                        diagnostics = (ImmutableArray<Diagnostic>)ex.Data["diagnostics"];
                    }
                    catch { }

                    Log.Error(ex, "Failed Compilation of class type '{type}'", data.type);
                    Log.Verbose($"\n{data.code}");

                    await Task.Delay(1000);

                    if (diagnostics.HasValue)
                    {
                        Console.WriteLine();
                        for (var i = 0; i < data.code.Length; i++)
                        {
                            var foundDiagnostic = diagnostics.Value.FirstOrDefault(x => i >= x.Location.SourceSpan.Start && i <= x.Location.SourceSpan.End, null);

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

                            Console.Write(data.code[i]);
                        }
                        Console.WriteLine();
                    }

                    _ = Console.ReadLine();
                }
            }
        }

        return assemblyList.ToArray();
    }

    internal static void RegisterAssemblies(Bot _bot, IReadOnlyDictionary<int, CommandsNextExtension> cNext, IReadOnlyDictionary<int, ApplicationCommandsExtension> appCommands, Action<ApplicationCommandsTranslationContext> translationContext, IEnumerable<(Assembly compiledAssembly, CompilationType type)> assemblyList)
    {
        foreach (var (compiledAssembly, type) in assemblyList)
        {
            foreach (var parentType in compiledAssembly.GetTypes())
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

        foreach (var (compiledAssembly, type) in assemblyList)
        {
            switch (type)
            {
                case CompilationType.Prefix:
                    cNext.RegisterCommands(compiledAssembly.GetTypes().First(x => x.BaseType == typeof(BaseCommandModule)));
                    break;

                case CompilationType.App:
                    if (_bot.status.LoadedConfig.IsDev)
                        appCommands.RegisterGuildCommands(compiledAssembly.GetTypes().First(x => x.BaseType == typeof(ApplicationCommandsModule)), _bot.status.LoadedConfig.Discord.DevelopmentGuild, translationContext);
                    else
                        appCommands.RegisterGlobalCommands(compiledAssembly.GetTypes().First(x => x.BaseType == typeof(ApplicationCommandsModule)), translationContext);
                    break;
            }
        }
    }

    private static string GetUniqueCodeCompatibleName()
        => $"a{Guid.NewGuid().ToString().ToLower().Replace("-", "")}";

    private static string? FileHeaderCache { get; set; } = null;
    private static string GetFileHeader()
    {
        FileHeaderCache ??= """
            // This file was auto generated and is part of Project Makoto.

            namespace ProjectMakoto;

            """ + string.Join("\n", File.ReadAllText("Global.cs").ReplaceLineEndings("\n").Split("\n").Where(x => !x.StartsWith("//"))).Replace("global ", "");

        return FileHeaderCache;
    }
}
