using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using TradeX.Infrastrucure.JwtAuthorization.Interfaces;
using TradeX.Infrastrucure.JwtAuthorization.Configuration;

namespace TradeX.Infrastrucure.JwtAuthorization.Services;

internal class TokenValidatorService : ITokenValidatorService
{
    private readonly TokenConfiguration _tokenConfiguration;
    private readonly IOpenIdConnectConfigurationReader _openIdConnectConfigurationReader;

    public TokenValidatorService(TokenConfiguration tokenConfiguration, IOpenIdConnectConfigurationReader openIdConnectConfigurationReader)
    {
        _tokenConfiguration = tokenConfiguration;
        _openIdConnectConfigurationReader = openIdConnectConfigurationReader;
    }

    public async Task<Tuple<IPrincipal?, SecurityToken?>> ValidateTokenAsync(string token)
    {
        ICollection<SecurityKey> signingKeys = await _openIdConnectConfigurationReader.GetSigningKeysAsync();
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var validationParameters = new TokenValidationParameters()
        {
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidIssuer = $"{_tokenConfiguration.IssuerScheme}://{_tokenConfiguration.Issuer}/",
            ValidAudience = _tokenConfiguration.Audience,
            IssuerSigningKeys = signingKeys
        };
        try
        {
            var result = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            return new Tuple<IPrincipal?, SecurityToken?>(result, validatedToken);
        }
        catch (SecurityTokenExpiredException ex)
        {
            throw new Exception("Token has expired.", ex);
        }
        catch (SecurityTokenInvalidIssuerException ex)
        {
            throw new Exception("Token has an invalid issuer.", ex);
        }
        catch (SecurityTokenInvalidAudienceException ex)
        {
            throw new Exception("Token has an invalid audience.", ex);
        }
        catch (SecurityTokenValidationException ex)
        {
            throw new Exception("Token validation failed: " + ex.Message, ex);
        }
        catch (SecurityTokenDecryptionFailedException ex)
        {
            throw new Exception("Token decryption failed: " + ex.Message, ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unknown error occurred: " + ex.Message, ex);
        }
    }
}
