namespace ProjectIchigo.Entities;

public class UrlSubmissions
{
    public UrlSubmissions(User user)
    {
        Parent = user;
    }
    private User Parent { get; set; }



    private bool _AcceptedTOS { get; set; } = false;
    public bool AcceptedTOS 
    { 
        get => _AcceptedTOS; 
        set 
        { 
            _AcceptedTOS = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "submission_accepted_tos", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private DateTime _LastTime { get; set; } = DateTime.MinValue;
    public DateTime LastTime 
    { 
        get => _LastTime; 
        set 
        { 
            _LastTime = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "submission_last_datetime", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    public List<string> AcceptedSubmissions { get; set; } = new();
}