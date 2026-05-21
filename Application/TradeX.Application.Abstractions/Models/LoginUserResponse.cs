using TradeX.Application.Abstractions.Enums;

namespace TradeX.Application.Abstractions.Models;

public class LoginUserResponse
{
    public string? UserIdentifier { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Message { get; set; }

    public LoginUserResult Result { get; set; }
    public string ResultDescription
    {
        get
        {
            return Enum.GetName(typeof(LoginUserResult), Result)!;
        }
    }
}
