using System.Text.RegularExpressions;

namespace API.Abstractions.Routing;

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value == null) return null;

        // Convert "Patients" -> "document-types"
        return Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
    }
}