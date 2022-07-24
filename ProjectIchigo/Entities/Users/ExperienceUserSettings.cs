namespace ProjectIchigo.Entities;

public class ExperienceUserSettings
{
    public ExperienceUserSettings(User user)
    {
        Parent = user;
    }
    private User Parent { get; set; }



    private bool _DirectMessageOptOut { get; set; } = false;
    public bool DirectMessageOptOut 
    { 
        get => _DirectMessageOptOut; 
        set 
        { 
            _DirectMessageOptOut = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "experience_directmessageoptout", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }
}
