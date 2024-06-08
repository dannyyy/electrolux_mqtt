using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Electrolux.Infrastructure.ApiClients.Gigya;

public class GigyaApiClient
{
    private const string SocializeIdsEndpoint = "https://socialize.eu1.gigya.com/socialize.getIDs";
    private const string AccountLoginEndpoint = "https://accounts.eu1.gigya.com/accounts.login";
    private const string AccountJwtEndpoint = "https://accounts.eu1.gigya.com/accounts.getJWT";
    private const string SocializeApiName = "Gigya-Socialize-Api";
    private const string AccountApiName = "Gigya-Account-Api";

    private readonly IHttpClientFactory _httpClientFactory;

    public GigyaApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SocializeIdsResponse> GetSocializeIdsAsync(string apiKey, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(SocializeApiName);

        var socializeIdsRequestModel = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("apiKey", apiKey),
            new KeyValuePair<string, string>("format", "json"),
            new KeyValuePair<string, string>("httpStatusCodes", "false"),
            new KeyValuePair<string, string>("sdk", "Android_6.2.1"),
            new KeyValuePair<string, string>("targetEnv", "mobile")
        });

        var response = await client.PostAsync(
            SocializeIdsEndpoint,
            socializeIdsRequestModel,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<SocializeIdsResponse>(cancellationToken: cancellationToken);
    }

    public async Task<LoginResponse> GetSessionInfo(
        string email,
        string password,
        string apiKey,
        string gmid,
        string ucid,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(AccountApiName);

        var accountLoginRequestModel = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("apiKey", apiKey),
            new KeyValuePair<string, string>("format", "json"),
            new KeyValuePair<string, string>("gmid", gmid),
            new KeyValuePair<string, string>("httpStatusCodes", "false"),
            new KeyValuePair<string, string>("loginID", email),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("sdk", "Android_6.2.1"),
            new KeyValuePair<string, string>("targetEnv", "mobile"),
            new KeyValuePair<string, string>("ucid", ucid)
        });

        var response = await client.PostAsync(
            AccountLoginEndpoint,
            accountLoginRequestModel,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
    }

    public async Task<JwtResponse> GetJwtToken(
        string apiKey,
        string gmid,
        string sessionToken,
        string sessionSecret,
        string ucid,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(AccountApiName);

        var parameters = new Dictionary<string, string>
        {
            {"apiKey", apiKey},
            {"fields", "country"},
            {"format", "json"},
            {"gmid", gmid},
            {"httpStatusCodes", "false"},
            {"nonce", $"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}_{Random.Shared.Next(int.MaxValue)}"},
            {"oauth_token", sessionToken},
            {"sdk", "Android_6.2.1"},
            {"targetEnv", "mobile"},
            {"ucid", ucid}
        };

        SignRequest(sessionSecret, "POST", AccountJwtEndpoint, parameters);
        var accountJwtRequestModel = new FormUrlEncodedContent(parameters);

        var response = await client.PostAsync(
            AccountJwtEndpoint,
            accountJwtRequestModel,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<JwtResponse>(cancellationToken: cancellationToken);
    }

    private void SignRequest(string secret, string method, string url, Dictionary<string, string> parameter)
    {
        var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        parameter.Add("timestamp", timeStamp.ToString());

        var uri = new Uri(url);
        var protocol = uri.Scheme;

        StringBuilder sb = new StringBuilder();
        sb.Append(protocol.ToLower());
        sb.Append("://");
        sb.Append(uri.Host.ToLower());
        sb.Append(uri.AbsolutePath);

        var queryString = string.Join("&", parameter.OrderBy(i => i.Key).Select(i => $"{i.Key}={Uri.EscapeDataString(i.Value)}"));
        var stringToSign = $"{method.ToUpper()}&{Uri.EscapeDataString(sb.ToString())}&{Uri.EscapeDataString(queryString)}";
        using var hmac = new HMACSHA1(Convert.FromBase64String(secret));
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
        parameter.Add("sig", signature);
    }
}