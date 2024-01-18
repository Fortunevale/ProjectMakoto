// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;
using ProjectMakoto.Entities.Members;

namespace ProjectMakoto.Entities.Guilds;

[TableName("-")]
public sealed class Member : RequiresParent<Guild>
{
    public Member(Bot bot, Guild guild, ulong key) : base(bot, guild)
    {
        this.InviteTracker = new(bot, this);
        this.Experience = new(bot, this);
        this.Id = key;

        _ = this.Bot.DatabaseClient.CreateRow(this.Parent.Id.ToString(), typeof(Member), key, this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    public async Task PerformAutoKickChecks(DiscordGuild guild = null, DiscordMember member = null, int retryCount = 0, List<Exception> exceptions = null)
    {
        exceptions ??= new();

        if (retryCount >= 3)
        {
            _logger.LogError("Failed to perform auto kick checks for {id}", this.Id);

            foreach (var b in exceptions)
                _logger.LogError("Auto Kick Error", b);

            throw exceptions.Last();
        }

        try
        {
            guild ??= await this.Bot.DiscordClient.GetShard(this.Parent.Id).GetGuildAsync(this.Parent.Id);
            member ??= await guild.GetMemberAsync(this.Id);
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
            await this.PerformAutoKickChecks(null, null, retryCount + 1, exceptions);
            return;
        }

        if (member.IsBot)
            return;

        if (this.Parent.Join.AutoKickSpammer && member.Flags.HasValue && 
            (member.Flags.Value.HasFlag(UserFlags.Spammer) || member.Flags.Value.HasFlag(UserFlags.DisabledSuspiciousActivity)))
        {
            await member.RemoveAsync(this.Bot.LoadedTranslations.Commands.Config.Join.AutoKickSpammerReason.Get(this.Parent));
            _logger.LogDebug("Kicked {User} from {Guild}: Account is likely spammer", this.Id, this.Parent.Id);
            return;
        }

        if (this.Parent.Join.AutoKickAccountAge != TimeSpan.Zero && 
            member.CreationTimestamp.GetTimespanSince() < this.Parent.Join.AutoKickAccountAge)
        {
            await member.RemoveAsync(this.Bot.LoadedTranslations.Commands.Config.Join.AutoKickAccountAgeReason.Get(this.Parent));
            _logger.LogDebug("Kicked {User} from {Guild}: Account is too young", this.Id, this.Parent.Id);
            return;
        }

        if (this.Parent.Join.AutoKickNoRoleTime != TimeSpan.Zero && member.JoinedAt.Add(this.Parent.Join.AutoKickNoRoleTime).GetTimespanSince() < this.Parent.Join.AutoKickNoRoleTime)
        {
            _ = new Func<Task>(async () =>
            {

                try
                {
                    member = await guild.GetMemberAsync(this.Id, true);

                    if (member.GetRoleHighestPosition() >= guild.CurrentMember.GetRoleHighestPosition())
                        return;

                    if (member.Roles.Count == 0)
                    {
                        await member.RemoveAsync(this.Bot.LoadedTranslations.Commands.Config.Join.AutoKickNoRolesReason.Get(this.Parent));
                        _logger.LogDebug("Kicked {User} from {Guild}: User did not pick roles after {Time}", this.Id, this.Parent.Id, this.Parent.Join.AutoKickNoRoleTime.GetHumanReadable());
                    }
                }
                catch (DisCatSharp.Exceptions.NotFoundException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Auto Kick Error", ex);
                    return;
                }
            }).CreateScheduledTask(member.JoinedAt.Add(this.Parent.Join.AutoKickNoRoleTime).UtcDateTime, 
                                   new ScheduledTaskIdentifier(this.Id, Guid.NewGuid().ToString(), "norolesautokick"));

            return;
        }
    }

    [ColumnName("userid"), ColumnType(ColumnTypes.BigInt), Primary]
    internal ulong Id { get; set; }

    [ColumnName("saved_nickname"), ColumnType(ColumnTypes.Text), Nullable]
    public string? SavedNickname
    {
        get => this.Bot.DatabaseClient.GetValue<string>(this.Parent.Id.ToString(), "userid", this.Id, "saved_nickname", this.Bot.DatabaseClient.guildDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Id.ToString(), "userid", this.Id, "saved_nickname", value, this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    [ColumnName("first_join"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime FirstJoinDate
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>(this.Parent.Id.ToString(), "userid", this.Id, "first_join", this.Bot.DatabaseClient.guildDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Id.ToString(), "userid", this.Id, "first_join", value, this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    [ColumnName("last_leave"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime LastLeaveDate
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>(this.Parent.Id.ToString(), "userid", this.Id, "last_leave", this.Bot.DatabaseClient.guildDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Id.ToString(), "userid", this.Id, "last_leave", value, this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    [ColumnName("roles"), ColumnType(ColumnTypes.LongText), Default("[]")]
    public MemberRole[] MemberRoles
    {
        get => JsonConvert.DeserializeObject<MemberRole[]>(this.Bot.DatabaseClient.GetValue<string>(this.Parent.Id.ToString(), "userid", this.Id, "roles", this.Bot.DatabaseClient.guildDatabaseConnection));
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Id.ToString(), "userid", this.Id, "roles", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    [ContainsValues]
    public InviteTrackerMember InviteTracker { get; init; }

    [ContainsValues]
    public ExperienceMember Experience { get; init; }
}