using System.Net.Http.Json;

namespace Electrolux.Infrastructure.ApiClients.Electrolux;

public class ElectroluxApiClient
{
    private const string AppApiKey = "2AMqwEV5MqVhTKrRCyYfVF8gmKrd2rAmp7cUsfky";
    private const string AppClientId = "ElxOneApp";
    private const string AppClientSecret = "8UKrsKD7jH9zvTV7rz5HeCLkit67Mmj68FvRVTlYygwJYy4dW6KF2cVLPKeWzUQUd6KJMtTifFf4NkDnjI7ZLdfnwcPtTSNtYvbP7OzEkmQD9IjhMOf5e1zeAQYtt2yN";
    private const string AgentName = "Ktor Client";

    private const string GenericApiName = "Electrolux-Generic-Api";
    private const string TokenEndpoint = "https://api.ocp.electrolux.one/one-account-authorization/api/v1/token";
    private const string MetadataEndpoint = "https://api.ocp.electrolux.one/one-account-user/api/v1/identity-providers";
    private const string AppliancesEndpoint = "https://api.ocp.electrolux.one/appliance/api/v2/appliances?includeMetadata=true";
    private const string ApplianceCapabilitiesEndpoint = "https://api.ocp.electrolux.one/appliance/api/v2/appliances/{0}/capabilities";
    private const string ApplianceStateEndpoint = "https://api.ocp.electrolux.one/appliance/api/v2/appliances/{0}";
    private const string CommandEndpoint = "https://api.ocp.electrolux.one/appliance/api/v2/appliances/{0}/command";

    private readonly IHttpClientFactory _httpClientFactory;

    public ElectroluxApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<JwtResponse> GetJwtTokenFromAppCredentialsAsync(CancellationToken cancellationToken)
    {
        var client = GetDefaultClient();

        var tokenRequestModel = new ElectroluxTokenRequest(
            "client_credentials",
            AppClientId,
            AppClientSecret,
            AgentName);

        var response = await client.PostAsJsonAsync(
            TokenEndpoint,
            tokenRequestModel,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<JwtResponse>(cancellationToken: cancellationToken);
    }

    public async Task<ApiMetadataResponse[]> GetApiMetadataAsync(string email, string accessToken, CancellationToken cancellationToken)
    {
        var client = GetDefaultClient(accessToken);

        var response = await client.GetAsync(
            $"{MetadataEndpoint}?brand=electrolux&email={email}",
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<ApiMetadataResponse[]>(cancellationToken: cancellationToken);
    }

    public async Task<JwtResponse> GetAccessTokenFromIdToken(string idToken, CancellationToken cancellationToken)
    {
        var client = GetDefaultClient();
        client.DefaultRequestHeaders.Add("Origin-Country-Code", "CH");

        var tokenExchangeRequestModel = new TokenExchangeRequestModel(
            grantType: "urn:ietf:params:oauth:grant-type:token-exchange",
            clientId: AppClientId,
            idToken: idToken,
            refreshToken: string.Empty,
            scope: string.Empty);

        var response = await client.PostAsJsonAsync(
            TokenEndpoint,
            tokenExchangeRequestModel,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<JwtResponse>(cancellationToken: cancellationToken);
    }

    public async Task<JwtResponse> GetAccessTokenFromRefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        var client = GetDefaultClient();

        var tokenExchangeRequestModel = new TokenExchangeRequestModel(
            grantType: "refresh_token",
            clientId: AppClientId,
            idToken: string.Empty,
            refreshToken: refreshToken,
            scope: string.Empty);

        var response = await client.PostAsJsonAsync(
            TokenEndpoint,
            tokenExchangeRequestModel,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<JwtResponse>(cancellationToken: cancellationToken);
    }

    public async Task<AppliancesResponse[]> GetAppliances(string accessToken, CancellationToken cancellationToken)
    {
        var client = GetDefaultClient(accessToken);

        var response = await client.GetAsync(
            AppliancesEndpoint,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<AppliancesResponse[]>(cancellationToken: cancellationToken);
    }

    public async Task<Dictionary<string, ApplianceCapabilitiesResponse>> GetApplianceCapabilitiesAsync(
        string accessToken,
        string applianceId,
        CancellationToken cancellationToken)
    {
        var client = GetDefaultClient(accessToken);

        var response = await client.GetAsync(
            string.Format(ApplianceCapabilitiesEndpoint, applianceId),
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<Dictionary<string, ApplianceCapabilitiesResponse>>(cancellationToken: cancellationToken);
    }

    public async Task<ApplianceStateResponse> GetApplianceStateAsync(
        string accessToken,
        string applianceId,
        CancellationToken cancellationToken)
    {
        var client = GetDefaultClient(accessToken);

        var response = await client.GetAsync(
            string.Format(ApplianceStateEndpoint, applianceId),
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<ApplianceStateResponse>(cancellationToken: cancellationToken);
    }

    public async Task SendCommandAsync(
        string accessToken,
        string applianceId,
        Dictionary<string, object> command,
        CancellationToken cancellationToken)
    {
        var client = GetDefaultClient(accessToken);

        var response = await client.PutAsJsonAsync(
            string.Format(CommandEndpoint, applianceId),
            command,
            cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private HttpClient GetDefaultClient(string? accessToken = null)
    {
        var client = _httpClientFactory.CreateClient(GenericApiName);
        client.DefaultRequestHeaders.Add("x-api-key", AppApiKey);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken ?? string.Empty}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Context-Brand", "electrolux");
        client.DefaultRequestHeaders.Add("User-Agent", "Ktor Client");

        return client;
    }
}