using TradeX.Application.Abstractions.Enums;

namespace TradeX.Application.Abstractions.QueryParameters;

public class FilterQueryParameters : List<FilterQueryParameter>
{
    public FilterQueryParameters()
    {

    }
}

public class FilterQueryParameter(string fieldName, List<FilterQueryExpression> filter)
{
    public string FieldName { get; set; } = fieldName;
    public List<FilterQueryExpression> Filter { get; set; } = filter;

    public FilterQueryParameterDeconstructed<T>? GetFilterQueryParameterDeconstructed<T>(Func<object?, T> convertValue)
    {
        var res = new FilterQueryParameterDeconstructed<T>()
        {
            Eq = convertValue(Filter.FirstOrDefault(x => x.Op == FilterQueryOperation.Eq)?.Value),
            Neq = convertValue(Filter.FirstOrDefault(x => x.Op == FilterQueryOperation.Neq)?.Value),
            Gt = convertValue(Filter.FirstOrDefault(x => x.Op == FilterQueryOperation.Gt)?.Value),
            Lt = convertValue(Filter.FirstOrDefault(x => x.Op == FilterQueryOperation.Lt)?.Value),
            Gte = convertValue(Filter.FirstOrDefault(x => x.Op == FilterQueryOperation.Gte)?.Value),
            Lte = convertValue(Filter.FirstOrDefault(x => x.Op == FilterQueryOperation.Lte)?.Value),
            StartsWith = convertValue(Filter.FirstOrDefault(x => x.Op == FilterQueryOperation.StartsWith)?.Value),
            Contains = convertValue(Filter.FirstOrDefault(x => x.Op == FilterQueryOperation.Contains)?.Value),
        };

        return res;
    }
}

public class FilterQueryExpression(FilterQueryOperation op, string value)
{
    public FilterQueryOperation Op { get; set; } = op;
    public string Value { get; set; } = value;
}

public class FilterQueryParameterDeconstructed<T>
{
    public T? Eq { get; set; }
    public T? Neq { get; set; }

    public T? Gt { get; set; }
    public T? Lt { get; set; }

    public T? Gte { get; set; }
    public T? Lte { get; set; }
    public T? StartsWith { get; set; }
    public T? Contains { get; set; }
}
