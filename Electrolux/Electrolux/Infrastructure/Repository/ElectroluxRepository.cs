using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Electrolux.Domain.UseCases.GetAppliances;
using Electrolux.Infrastructure.ApiClients;
using Electrolux.Infrastructure.ApiClients.Electrolux;
using Electrolux.Infrastructure.ApiClients.Gigya;
using Microsoft.Extensions.Options;

namespace Electrolux.Infrastructure.Repository;

public class ElectroluxRepository
{
    private const int RefreshThresholdInMinutes = 60;

    private readonly ElectroluxOptions _electroluxOptions;
    private readonly ElectroluxApiClient _electroluxApiClient;
    private readonly GigyaApiClient _gigyaApiClient;

    private string? _accessToken;
    private string? _refreshRoken;

    public ElectroluxRepository(
        IOptions<ElectroluxOptions> electroluxOptions,
        ElectroluxApiClient electroluxApiClient,
        GigyaApiClient gigyaApiClient)
    {
        _electroluxOptions = electroluxOptions.Value;
        _electroluxApiClient = electroluxApiClient;
        _gigyaApiClient = gigyaApiClient;
    }

    public async Task<IReadOnlySet<Appliance>> GetAppliances(CancellationToken cancellationToken)
    {
        await EnsureLoginAsync(cancellationToken);

        return (await _electroluxApiClient
            .GetAppliances(_accessToken!, cancellationToken))
            .Select(i => new Appliance(i.ApplianceId, i.ApplianceData.ApplianceName, i.ApplianceData.ModelName))
            .ToImmutableHashSet();
    }

    public async Task<IReadOnlySet<ApplianceCapability>> GetCapabilitiesByApplianceAsync(string applianceId, CancellationToken cancellationToken)
    {
        await EnsureLoginAsync(cancellationToken);
        var applianceCapabilities = (await _electroluxApiClient
                .GetApplianceCapabilitiesAsync(_accessToken!, applianceId, cancellationToken))
            .Where(i => i.Value.Access == "readwrite")
            .Select(i => new ApplianceCapability(i.Key, i.Value.Values?.Keys.Select(k => k).ToImmutableHashSet(),
                i.Value.Min, i.Value.Max))
            .Append(new ApplianceCapability("executeCommand", ImmutableHashSet.Create("ON", "OFF")));

        return applianceCapabilities
            .DistinctBy(i => i.command)
            .ToImmutableHashSet();
    }

    public async Task<ApplianceState> GetStateByApplianceAsync(string applianceId, CancellationToken cancellationToken)
    {
        await EnsureLoginAsync(cancellationToken);
        var applianceState = await _electroluxApiClient
            .GetApplianceStateAsync(_accessToken!, applianceId, cancellationToken);

        var keyValue = ImmutableDictionary.Create<string, object>()
            .AddRange(applianceState
                .Properties.Reported.Where(i => !i.Key.StartsWith("$"))
                .ToImmutableDictionary(i => i.Key, i => i.Value))
            .Add(JsonNamingPolicy.CamelCase.ConvertName(nameof(applianceState.ConnectionState)), applianceState.ConnectionState.ToLower());

        return new ApplianceState(keyValue);
    }

    public async Task SendCommandAsync(string applianceId, ApplianceCommand applianceCommand, CancellationToken cancellationToken)
    {
        await EnsureLoginAsync(cancellationToken);
        await _electroluxApiClient.SendCommandAsync(
            _accessToken!,
            applianceId,
            new Dictionary<string, object> { {applianceCommand.Command, applianceCommand.Value} },
            cancellationToken);
    }

    private async Task EnsureLoginAsync(CancellationToken cancellationToken)
    {
        if (IsAccessTokenValid())
            return;

        JwtResponse jwtResponse;
        if (!IsAccessTokenValid() && !string.IsNullOrWhiteSpace(_refreshRoken))
        {
            jwtResponse = await _electroluxApiClient.GetAccessTokenFromRefreshToken(_refreshRoken, cancellationToken);
            _accessToken = jwtResponse.AccessToken;
            _refreshRoken = jwtResponse.RefreshToken;

            return;
        }

        jwtResponse = await _electroluxApiClient.GetJwtTokenFromAppCredentialsAsync(cancellationToken);
        var apiMetadataResponse = await _electroluxApiClient.GetApiMetadataAsync(_electroluxOptions.Email, jwtResponse.AccessToken, cancellationToken);
        var socializeIdResponse = await _gigyaApiClient.GetSocializeIdsAsync(apiMetadataResponse[0].ApiKey, cancellationToken);
        var loginResponse = await _gigyaApiClient.GetSessionInfo(_electroluxOptions.Email, _electroluxOptions.Password, apiMetadataResponse[0].ApiKey, socializeIdResponse.Gmid, socializeIdResponse.Ucid, cancellationToken);
        jwtResponse = await _gigyaApiClient.GetJwtToken(apiMetadataResponse[0].ApiKey, socializeIdResponse.Gmid, loginResponse.SessionInfo.SessionToken, loginResponse.SessionInfo.SessionSecret, socializeIdResponse.Ucid, cancellationToken);
        jwtResponse = await _electroluxApiClient.GetAccessTokenFromIdToken(jwtResponse.IdToken, cancellationToken);

        _accessToken = jwtResponse.AccessToken;
        _refreshRoken = jwtResponse.RefreshToken;
    }

    private bool IsAccessTokenValid()
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
            return false;

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(_accessToken);
        var token = (JwtSecurityToken)jsonToken;

        return token.ValidTo > DateTime.UtcNow.AddMinutes(RefreshThresholdInMinutes);
    }
}