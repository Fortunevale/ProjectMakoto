namespace ProjectMakoto.Entities;

public class InviteTrackerMember
{
    public InviteTrackerMember(Member member)
    {
        Parent = member;
    }

    private Member Parent { get; set; }



    private ulong _UserId { get; set; } = 0;
    public ulong UserId 
    { 
        get => _UserId; 
        set 
        { 
            _UserId = value;
            _ = Bot.DatabaseClient.UpdateValue(Parent.Guild.ServerId.ToString(), "userid", Parent.MemberId, "invite_user", value, Bot.DatabaseClient.guildDatabaseConnection);
        } 
    }

    private string _Code { get; set; } = "";
    public string Code 
    { 
        get => _Code; 
        set 
        { 
            _Code = value;
            _ = Bot.DatabaseClient.UpdateValue(Parent.Guild.ServerId.ToString(), "userid", Parent.MemberId, "invite_code", value, Bot.DatabaseClient.guildDatabaseConnection);
        } 
    }
}
