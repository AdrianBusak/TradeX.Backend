using System.Globalization;

namespace TradeX.Application.Abstractions.Extensions;

public static class StringExtensions
{
    public static string FirstCharToLowerInvariant(this string str)
    {
        return str[..1].ToLowerInvariant() + str[1..];
    }

    public static string FirstCharToUpperInvariant(this string str)
    {
        return str[..1].ToUpperInvariant() + str[1..];
    }

    public static DateOnly? ToDateOnly(this string str)
    {
        if (str == null)
        {
            return null;
        }

        return DateOnly.Parse(str);
    }

    public static int? ToInt(this string? str)
    {
        if (str == null)
        {
            return null;
        }

        return int.Parse(str);
    }

    public static Guid? ToGuid(this string? str)
    {
        if (str == null)
        {
            return null;
        }

        return Guid.Parse(str);
    }

    public static long? ToLong(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (long.TryParse(value, out var result))
            return result;

        throw new FormatException("Invalid long value.");
    }

    

    private const string IsoDateFormat = "yyyy-MM-dd";

    public static DateTime? ToIsoDateOrNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateTime.TryParseExact(
                value,
                IsoDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            return parsed.Date;
        }

        throw new FormatException($"Date must be in ISO format {IsoDateFormat}.");
    }

    public static DateTime ToIsoDate(this string value)
    {
        var result = value.ToIsoDateOrNull();
        return result is null ? throw new FormatException($"Date must be in ISO format {IsoDateFormat}.") : result.Value;
    }
}
