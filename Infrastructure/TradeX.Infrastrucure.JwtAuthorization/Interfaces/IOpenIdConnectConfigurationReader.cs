using Microsoft.IdentityModel.Tokens;

namespace TradeX.Infrastrucure.JwtAuthorization.Interfaces;

public interface IOpenIdConnectConfigurationReader
{
    Task<ICollection<SecurityKey>> GetSigningKeysAsync();
}
