using System.ComponentModel;

namespace TradeX.Application.Abstractions.Enums;

public enum RegistrationStatus
{
    [Description("Pending")]
    Pending = 0,
    [Description("Active")]
    Active = 1,
}