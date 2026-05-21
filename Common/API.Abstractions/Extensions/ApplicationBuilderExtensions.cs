using Asp.Versioning.ApiExplorer;

namespace API.Abstractions.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UseVersionedSwaggerUI(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        app.UseSwagger();

        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"TradeX API {description.GroupName.ToUpperInvariant()}"
                );
            }
        });
    }
}
