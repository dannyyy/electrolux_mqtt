namespace Electrolux.Infrastructure.ApiClients.Electrolux;

[Serializable]
public class TokenExchangeRequestModel
{
    public TokenExchangeRequestModel(string grantType, string clientId, string idToken, string refreshToken, string scope)
    {
        GrantType = grantType;
        ClientId = clientId;
        IdToken = idToken;
        RefreshToken = refreshToken;
        Scope = scope;
    }

    public string GrantType { get; set; }
    public string ClientId { get; set; }
    public string IdToken { get; set; }
    public string RefreshToken { get; set; }
    public string Scope { get; set; }
}