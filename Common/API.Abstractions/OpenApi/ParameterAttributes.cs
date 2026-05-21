using Swashbuckle.AspNetCore.Annotations;

namespace API.Abstractions.OpenApi;

/// <summary>
/// Custom Swagger attribute to provide a descriptive explanation for filtering query parameters.
/// This attribute overrides the default parameter description in the generated documentation.
/// </summary>
public class SwaggerFilterDescriptionAttribute : SwaggerParameterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerFilterDescriptionAttribute"/> class.
    /// Sets the detailed JSON format example for filtering.
    /// </summary>
    public SwaggerFilterDescriptionAttribute() : base("Example: [{\"FieldName\":\"name\",\"Filter\":[{\"Op\":\"Contains\",\"Value\":\"abc\"}]}]")
    {
        Required = false;
    }
}

/// <summary>
/// Custom Swagger attribute to provide a descriptive explanation for sorting query parameters.
/// </summary>
public class SwaggerSortDescriptionAttribute : SwaggerParameterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerSortDescriptionAttribute"/> class.
    /// Sets the detailed JSON format example for sorting.
    /// </summary>
    public SwaggerSortDescriptionAttribute() : base(
        description: "Example: [{\"FieldName\":\"createdAt\",\"Direction\":\"Desc\"}]")
    {
        Required = false;
    }
}

/// <summary>
/// Custom Swagger attribute to provide a descriptive explanation for paging query parameters.
/// </summary>
public class SwaggerPagingDescriptionAttribute : SwaggerParameterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerPagingDescriptionAttribute"/> class.
    /// Sets the detailed JSON format example for paging.
    /// </summary>
    public SwaggerPagingDescriptionAttribute() : base(
        description: "Example: {\"Index\":0,\"Size\":10}")
    {
        Required = false;
    }
}