using TradeX.Application.Abstractions.QueryParameters;

namespace TradeX.Application.Abstractions.Extensions;

public static class FilterQueryParametersExtensions
{
    public static FilterQueryParameterDeconstructed<DateTime?>? GetDateFilter(this FilterQueryParameters filterParameters, string fieldName)
    {
        return filterParameters?
            .FirstOrDefault(x => x.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase))?
            .GetFilterQueryParameterDeconstructed(value => value?.ToString().ToIsoDateOrNull());
    }

    public static FilterQueryParameterDeconstructed<Guid?>? GetGuidFilter(this FilterQueryParameters filterParameters, string fieldName)
    {
        return filterParameters?
            .FirstOrDefault(x => x.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase))?
            .GetFilterQueryParameterDeconstructed(value => value?.ToString().ToGuid());
    }

    public static FilterQueryParameterDeconstructed<int?>? GetIntFilter(this FilterQueryParameters filterParameters, string fieldName)
    {
        return filterParameters?
            .FirstOrDefault(x => x.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase))?
            .GetFilterQueryParameterDeconstructed(value => value?.ToString().ToInt());
    }

    public static FilterQueryParameterDeconstructed<long?>? GetLongFilter(this FilterQueryParameters filterParameters, string fieldName)
    {
        return filterParameters?
            .FirstOrDefault(x => x.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase))?
            .GetFilterQueryParameterDeconstructed(value => value?.ToString().ToLong());
    }

    public static FilterQueryParameterDeconstructed<string?>? GetStringFilter(this FilterQueryParameters filterParameters, string fieldName)
    {
        return filterParameters?
            .FirstOrDefault(x => x.FieldName.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase))?
            .GetFilterQueryParameterDeconstructed((value) => (string?)value);
    }

    public static FilterQueryParameterDeconstructed<bool?>? GetBoolFilter(
        this FilterQueryParameters filterParameters,
        string fieldName)
    {
        return filterParameters?
            .FirstOrDefault(x => x.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase))?
            .GetFilterQueryParameterDeconstructed(value =>
            {
                if (value == null)
                    return (bool?) null;

                return bool.TryParse(value.ToString(), out var parsed)
                    ? parsed
                    : null;
            });
    }



}
