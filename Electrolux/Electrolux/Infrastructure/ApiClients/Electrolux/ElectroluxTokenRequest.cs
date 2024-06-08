namespace Electrolux.Infrastructure.ApiClients.Electrolux;

[Serializable]
public class ElectroluxTokenRequest
{
    public ElectroluxTokenRequest(string grantType, string clientId, string clientSecret, string scope)
    {
        GrantType = grantType;
        ClientId = clientId;
        ClientSecret = clientSecret;
        Scope = scope;
    }

    public string GrantType { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Scope { get; set; }
}