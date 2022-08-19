namespace ProjectIchigo.Entities;

public class UrlSubmissionSettings
{
    public UrlSubmissionSettings(User user)
    {
        Parent = user;
    }
    private User Parent { get; set; }



    private int _AcceptedTOS { get; set; } = 0;
    public int AcceptedTOS 
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