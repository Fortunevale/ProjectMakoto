using ProjectMakoto.Entities.EpicGames;
using System.Net.Http.Headers;
using ProjectMakoto.Util.EpicGames;

namespace ProjectMakoto.Util;

public class EpicGamesClient
{
    private EpicGamesClient() { }
    
    internal Bot _bot { get; private set; }
    private Config.SecretsConfig.EpicGamesSecrets EpicGamesSecrets 
        => _bot.status.LoadedConfig.Secrets.EpicGames;

    private string ClientIdAndSecret
        => Encoding.UTF8.GetString(Convert.FromBase64String(EpicGamesSecrets.EpicClient));
    
    private string ClientId
        => ClientIdAndSecret[..(ClientIdAndSecret.IndexOf(":"))];

    public static EpicGamesClient Initialize(Bot bot)
    {
        EpicGamesClient client = new()
        {
            _bot = bot
        };

        return client;
    }

    public async Task UpdateCookie(string EpicDevice, string EpicSession)
    {
        if ((await GetAuthCode(EpicDevice, EpicSession)).Item1)
        {
            EpicGamesSecrets.Cookies.EPIC_DEVICE = EpicDevice;
            EpicGamesSecrets.Cookies.EPIC_SESSION_AP = EpicSession;

            _bot.status.LoadedConfig.Save();
        }
        else
            throw new ArgumentException("The entered cookies are not functional.");
    }

    public async Task<(bool, string?)> GetAuthCode(string EpicDevice, string EpicSession)
    {
        CookieContainer testContainer = new();

        HttpClient testClient = new HttpClient(new HttpClientHandler
        {
            CookieContainer = testContainer,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        }).InitializeClientWithDefaultHeaders(this);

        testContainer.Add(new Uri("https://www.epicgames.com"), new Cookie("EPIC_DEVICE", EpicDevice));
        testContainer.Add(new Uri("https://www.epicgames.com"), new Cookie("EPIC_SESSION_AP", EpicSession));

        _logger.LogDebug("Testing Epic Games Cookies..");
        var rawResponse = await testClient.GetAsync($"https://www.epicgames.com/id/api/redirect?clientId={ClientId}&responseType=code");
        AuthorizationCode parsedResponse;

        try
        {
            var value = await rawResponse.Content.ReadAsStringAsync();
            parsedResponse = JsonConvert.DeserializeObject<AuthorizationCode>(value);

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

        HttpClient client = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        }).InitializeClientWithDefaultHeaders(this);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", EpicGamesSecrets.EpicClient);
        
        var rawResponse = await client.PostAsync("https://account-public-service-prod.ol.epicgames.com/account/api/oauth/token", content);

        OAuthToken oAuthToken;
        try
        {
            var value = await rawResponse.Content.ReadAsStringAsync();
            if (!rawResponse.IsSuccessStatusCode)
            {
                ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(value);
                throw new Exception(errorMessage?.errorMessage ?? "Unknown Error occured.");
            }

            oAuthToken = JsonConvert.DeserializeObject<OAuthToken>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to retrieve OAuthToken from Epic Games", ex);
            throw;
        }

        CachedToken = oAuthToken;
        return oAuthToken;
    }

    public async Task ClaimDailyRewards()
    {
        if (EpicGamesSecrets.LastDailyClaim.ToString("dd.MM.yyyy") == DateTime.UtcNow.ToString("dd.MM.yyyy"))
        {
            return;
        }

        var oAuthCode = await GetOAuthToken();

        HttpClient client = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        }).InitializeClientWithDefaultHeaders(this);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oAuthCode.access_token);

        var content = new StringContent("{}");

        var rawResponse = await client.PostAsync($"https://fortnite-public-service-prod11.ol.epicgames.com/fortnite/api/game/v2/profile/{oAuthCode.account_id}/client/ClaimLoginReward?profileId=campaign", content);

        object Response;
        try
        {
            if (!rawResponse.IsSuccessStatusCode)
            {
                if ((int)rawResponse.StatusCode == 415)
                {
                    _logger.LogError("Daily Rewards were already claimed.");

                    EpicGamesSecrets.LastDailyClaim = DateTime.UtcNow;
                    _bot.status.LoadedConfig.Save();
                    return;
                }

                throw new Exception($"Non-Success Status Code: {rawResponse.StatusCode}");
            }

            EpicGamesSecrets.LastDailyClaim = DateTime.UtcNow;
            _bot.status.LoadedConfig.Save();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to claim daily rewards", ex);
            _logger.LogError("{0}", JsonConvert.SerializeObject(rawResponse, Formatting.Indented));
            throw;
        }
    }
}
