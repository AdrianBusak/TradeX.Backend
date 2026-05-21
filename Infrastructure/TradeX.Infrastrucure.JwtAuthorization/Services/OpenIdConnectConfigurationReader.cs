using TradeX.Infrastrucure.JwtAuthorization.Configuration;
using TradeX.Infrastrucure.JwtAuthorization.Interfaces;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace TradeX.Infrastrucure.JwtAuthorization.Services;

internal class OpenIdConnectConfigurationReader: IOpenIdConnectConfigurationReader
{
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _manager;
    private readonly TokenConfiguration _tokenConfiguration;

    public OpenIdConnectConfigurationReader(TokenConfiguration tokenConfiguration)
    {
        _tokenConfiguration = tokenConfiguration;
        ArgumentException.ThrowIfNullOrWhiteSpace(_tokenConfiguration.Issuer);
        
        string endpoint = $"{_tokenConfiguration.IssuerScheme!}://{_tokenConfiguration.Issuer!}/.well-known/openid-configuration";

        _manager = new ConfigurationManager<OpenIdConnectConfiguration>(
            endpoint,
            new OpenIdConnectConfigurationRetriever())
            {
                AutomaticRefreshInterval = TimeSpan.FromHours(1),
                RefreshInterval = TimeSpan.FromMinutes(5)
            };
    }

    public async Task<ICollection<SecurityKey>> GetSigningKeysAsync()
    {
        try
        {
            var openIdConfig = await _manager.GetConfigurationAsync(CancellationToken.None);

            return openIdConfig.SigningKeys;
        }
        catch (InvalidOperationException invalidOperationException)
        {
            Exception ex = invalidOperationException;
            
            if (invalidOperationException.InnerException != null)
            {
                ex = invalidOperationException.InnerException;
            }

            throw new Exception($"Error getting signing keys from IDP. [Endpoint: {_manager.MetadataAddress}]", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting signing keys from IDP. [Endpoint: {_manager.MetadataAddress}]", ex);
        }
    }
}
