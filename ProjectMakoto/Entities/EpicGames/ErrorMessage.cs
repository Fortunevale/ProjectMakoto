namespace ProjectMakoto.Entities.EpicGames;

public class ErrorMessage
{
    public string errorCode { get; set; }
    public string errorMessage { get; set; }
    public object[] messageVars { get; set; }
    public int numericErrorCode { get; set; }
    public string originatingService { get; set; }
    public string intent { get; set; }
    public string error_description { get; set; }
    public string error { get; set; }
}
