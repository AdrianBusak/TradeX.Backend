using TradeX.Application.Abstractions.Interfaces;
using TradeX.Infrastrucure.JwtAuthorization.Interfaces;

namespace API.Abstractions.Services;

public class UserContextAccessor(IHttpContextAccessor httpContextAccessor, ITokenValidatorService tokenValidatorService) : BaseContextAccessor(httpContextAccessor, tokenValidatorService), IUserContextAccessor
{
    public async Task<Tuple<string?, bool>> GetUserIdentifierAsync()
    {
        var principal = await GetPrincipalFromHeaderAsync();
        var userIdentifier = GetClaimValue(principal, System.Security.Claims.ClaimTypes.NameIdentifier);
        string? strIsActive = GetClaimValue(principal, "isActive");
        bool isActive = true;

        if (!string.IsNullOrWhiteSpace(strIsActive))
        {
            _ = bool.TryParse(strIsActive, out isActive);
        }

        return new Tuple<string?, bool>(userIdentifier, isActive);
    }
}
