namespace ProjectIchigo.Entities;

public class AfkStatus
{
    public AfkStatus(User user)
    {
        Parent = user;
    }
    private User Parent { get; set; }



    private string _Reason { get; set; } = "";
    public string Reason 
    { 
        get => _Reason; 
        set 
        { 
            _Reason = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "afk_reason", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }



    private DateTime _TimeStamp { get; set; } = DateTime.UnixEpoch;
    public DateTime TimeStamp 
    { 
        get => _TimeStamp; 
        set 
        { 
            _TimeStamp = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "afk_since", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private long _MessagesAmount { get; set; } = 0;
    public long MessagesAmount 
    { 
        get => _MessagesAmount; 
        set 
        { 
            _MessagesAmount = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "afk_pingamount", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }



    private List<MessageDetails> _Messages { get; set; } = new();
    public List<MessageDetails> Messages
    {
        get
        {
            _Messages ??= new();

            return _Messages;
        }

        set
        {
            _Messages = value;
        }
    }

    [JsonIgnore]
    internal DateTime LastMentionTrigger { get; set; } = DateTime.MinValue;
}
