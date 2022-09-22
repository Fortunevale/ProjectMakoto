namespace ProjectIchigo.Entities;

public class RequestItem
{
    public string Url { get; set; }

    public string Response { get; set; }

    public bool Resolved { get; set; }
    public bool Failed { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public Exception Exception { get; set; }
}