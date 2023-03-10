using ProjectMakoto.Entities.EpicGames;
using System.Net.Http.Headers;

namespace ProjectMakoto.Util;

public class EpicGamesClient
{
    private EpicGamesClient() { }
    
    private Bot _bot { get; set; }
    private Config.SecretsConfig.EpicGamesSecrets EpicGamesSecrets 
        => _bot.status.LoadedConfig.Secrets.EpicGames;

    private string ClientIdAndSecret
        => Encoding.UTF8.GetString(Convert.FromBase64String(EpicGamesSecrets.EpicClient));
    
    private string ClientId
        => ClientIdAndSecret[..(ClientIdAndSecret.IndexOf(":"))];
    
    private string ClientSecret
        => ClientIdAndSecret[(ClientIdAndSecret.IndexOf(":") + 1)..];

    public static EpicGamesClient Initialize(Bot bot)
    {
        EpicGamesClient epicGamesClient = new()
        {
            _bot = bot
        };

        return epicGamesClient;
    }

    public async Task UpdateCookie(string EpicDevice, string EpicSession)
    {
        if ((await GetAuthCode(EpicDevice, EpicSession)).Item1)
        {
            EpicGamesSecrets.Cookies.EPIC_DEVICE = EpicDevice;
            EpicGamesSecrets.Cookies.EPIC_SESSION_AP = EpicSession;
        }
        else
            throw new ArgumentException("The entered cookies are not functional.");
    }

    public async Task<(bool, string?)> GetAuthCode(string EpicDevice, string EpicSession)
    {
        CookieContainer testContainer = new();

        HttpClient testClient = new HttpClient(new HttpClientHandler
        {
            CookieContainer = testContainer
        });

        testContainer.Add(new Uri("https://www.epicgames.com"), new Cookie("EPIC_DEVICE", EpicDevice));
        testContainer.Add(new Uri("https://www.epicgames.com"), new Cookie("EPIC_SESSION_AP", EpicSession));

        _logger.LogDebug("Testing Epic Games Cookies..");
        var rawResponse = await testClient.GetAsync($"https://www.epicgames.com/id/api/redirect?clientId={ClientId}&responseType=code");
        AuthorizationCode parsedResponse;

        try
        {
            parsedResponse = JsonConvert.DeserializeObject<AuthorizationCode>(await rawResponse.Content.ReadAsStringAsync());

            if (parsedResponse is null)
                throw new Exception("Invalid Json");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to parse Epic Games Cookie Test Response", ex);
            return (false, null);
        }

        if (parsedResponse.authorizationCode.IsNullOrWhiteSpace())
        {
            return (false, null);
        }

        return (true, parsedResponse.authorizationCode);
    }

    private OAuthToken CachedToken = null;
    public async Task<OAuthToken> GetOAuthToken(bool Refetch = false)
    {
        if (!Refetch && (CachedToken?.expires_at ?? DateTime.MinValue) > DateTime.UtcNow)
            return CachedToken;

        (bool AuthCodeSuccess, string AuthCode) = await GetAuthCode(EpicGamesSecrets.Cookies.EPIC_DEVICE, EpicGamesSecrets.Cookies.EPIC_SESSION_AP);

        if (!AuthCodeSuccess)
            throw new Exception("Retrieving Authentication Code failed.");

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", AuthCode),
        });

        HttpClient client = new();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", EpicGamesSecrets.EpicClient);
        client.DefaultRequestHeaders.Add("User-Agent", $"ProjectMakoto/{_bot.status.LoadedConfig.DontModify.LastStartedVersion}");
        client.DefaultRequestHeaders.Add("Accept", "*/*");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");


        var rawResponse = await client.PostAsync("https://account-public-service-prod.ol.epicgames.com/account/api/oauth/token", content);

        OAuthToken oAuthToken;
        try
        {
            if (!rawResponse.IsSuccessStatusCode)
            {
                ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(await rawResponse.Content.ReadAsStringAsync());
                throw new Exception(errorMessage?.errorMessage ?? "Unknown Error occured.");
            }

            oAuthToken = JsonConvert.DeserializeObject<OAuthToken>(await rawResponse.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to retrieve OAuthToken from Epic Games", ex);
            throw;
        }

        CachedToken = oAuthToken;
        return oAuthToken;
    }

    public async Task
}
