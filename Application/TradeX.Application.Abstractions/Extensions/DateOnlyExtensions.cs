namespace TradeX.Application.Abstractions.Extensions;

public static class DateOnlyExtensions
{
    public static DateTime ToDateTime(this DateOnly date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
    }

    public static DateTimeOffset? ToDateTimeOffset(this DateOnly? date)
    {
        if (!date.HasValue)
        {
            return null;
        }

        return date.Value.ToDateTimeOffset();
    }

    public static DateTimeOffset ToDateTimeOffset(this DateOnly date)
    {
        return new DateTimeOffset(date.ToDateTime(), new TimeSpan(0L));
    }

    public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime, new TimeSpan(0L));
    }
}
