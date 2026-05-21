using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.RegularExpressions;

namespace API.Abstractions.OpenApi;

public class SlugifyDocumentFilter : IDocumentFilter
{
    private static string Slugify(string value)
    {
        return Regex.Replace(value, "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var newTags = new List<OpenApiTag>();

        foreach (var tag in swaggerDoc.Tags)
        {
            newTags.Add(new OpenApiTag
            {
                Name = Slugify(tag.Name),
                Description = tag.Description
            });
        }

        swaggerDoc.Tags = newTags;

        // Also update each path's tag list
        foreach (var path in swaggerDoc.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                operation.Value.Tags = operation.Value.Tags
                    .Select(t => new OpenApiTag { Name = Slugify(t.Name) })
                    .ToList();
            }
        }
    }
}
