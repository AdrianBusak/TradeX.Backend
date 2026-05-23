using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Constants;
using TradeX.Application.Abstractions.Models;
using TradeX.Infrastrucure.JwtAuthorization.Interfaces;
using System.Security.Claims;

namespace API.Abstractions.Services;

public class UserContextAccessor(IHttpContextAccessor httpContextAccessor, ITokenValidatorService tokenValidatorService) : BaseContextAccessor(httpContextAccessor, tokenValidatorService), IUserContextAccessor
{
    public async Task<AuthenticatedUserContext> GetAuthenticatedUserAsync()
    {
        var principal = await GetPrincipalFromHeaderAsync();
        var externalUserId = GetClaimValue(principal, ClaimTypes.NameIdentifier)
            ?? GetClaimValue(principal, "sub");
        string? strIsActive = GetClaimValue(principal, CustomClaimTypes.IsActive);
        bool isActive = true;

        if (!string.IsNullOrWhiteSpace(strIsActive))
        {
            _ = bool.TryParse(strIsActive, out isActive);
        }

        return new AuthenticatedUserContext(
            externalUserId,
            GetClaimValue(principal, CustomClaimTypes.Email),
            GetClaimValue(principal, CustomClaimTypes.FirstName),
            GetClaimValue(principal, CustomClaimTypes.LastName),
            isActive);
    }
}
