using TradeX.Infrastrucure.JwtAuthorization.Interfaces;
using System.Security.Claims;
using System.Security.Principal;

namespace API.Abstractions.Services;

public abstract class BaseContextAccessor
{
    private readonly ITokenValidatorService _tokenValidatorService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected BaseContextAccessor(
        IHttpContextAccessor httpContextAccessor,
        ITokenValidatorService tokenValidatorService)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenValidatorService = tokenValidatorService;
    }

    protected string? GetHeaderValue(string header)
    {
        return GetHeaderValues(header)?.FirstOrDefault();
    }

    protected List<string?> GetHeaderValues(string headerKey)
    {
        var headerValues = new List<string?>();

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null && request.Headers.TryGetValue(headerKey, out var values))
        {
            headerValues.AddRange(values);
        }

        return headerValues;
    }

    protected async Task<IPrincipal?> GetPrincipalFromHeaderAsync()
    {
        var authorizationHeader = GetHeaderValue("Authorization");

        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var bearerToken = authorizationHeader["Bearer ".Length..].Trim();
            var result = await _tokenValidatorService.ValidateTokenAsync(bearerToken);

            return result.Item1;
        }

        return null;
    }

    protected string? GetClaimValue(IPrincipal? principal, string claimType)
    {
        if (principal is ClaimsPrincipal p)
        {
            var claim = p.FindFirst(claimType);
            return claim?.Value;
        }

        return null;
    }
}
