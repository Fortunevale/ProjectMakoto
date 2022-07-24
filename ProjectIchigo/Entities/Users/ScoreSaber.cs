namespace ProjectIchigo.Entities;

public class ScoreSaber
{
    public ScoreSaber(User user)
    {
        Parent = user;
    }
    private User Parent { get; set; }



    private ulong _Id { get; set; } = 0;
    public ulong Id 
    { 
        get => _Id; 
        set 
        { 
            _Id = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "scoresaber_id", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }
}
