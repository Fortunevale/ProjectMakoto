namespace ProjectMakoto.Entities.EpicGames;

public class AuthorizationCode
{
    public string redirectUrl { get; set; }
    public string? authorizationCode { get; set; }
    public object sid { get; set; }
}
