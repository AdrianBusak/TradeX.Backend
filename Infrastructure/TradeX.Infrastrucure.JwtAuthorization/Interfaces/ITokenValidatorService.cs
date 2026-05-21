using Microsoft.IdentityModel.Tokens;
using System.Security.Principal;

namespace TradeX.Infrastrucure.JwtAuthorization.Interfaces;

public interface ITokenValidatorService
{
    Task<Tuple<IPrincipal?, SecurityToken?>> ValidateTokenAsync(string token);
}
