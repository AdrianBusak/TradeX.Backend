using API.Abstractions.OpenApi;
using API.Abstractions.Routing;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

namespace API.Abstractions.Extensions;

public static class ServiceCollectionExtensions
{
    
    public static IServiceCollection AddControllersWithJsonOptions(this IServiceCollection services)
    {
        services
            .AddControllers(options =>
            {
                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
            })
            .AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });

        return services;
    }

    public static IServiceCollection AddApiVersioningAndExplorer(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            //options.GroupNameFormat = "'v'VV";
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        })
        ;

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, string title)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            //c.SwaggerDoc("v1", new OpenApiInfo { Title = "TradeX API", Version = "v1" });
            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

            var uniqueGroups = provider.ApiVersionDescriptions
                 .GroupBy(d => d.GroupName)
                 .Select(g => g.First())
                 .ToList();

            if (uniqueGroups.Count == 0)
            {
                // fallback — if no versioned groups exist, create a default one
                c.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1" });
            }
            else
            {
                foreach (var description in uniqueGroups)
                {
                    c.SwaggerDoc(description.GroupName, new OpenApiInfo
                    {
                        Title = title,
                        Version = description.ApiVersion.ToString()
                    });
                }
            }

            c.EnableAnnotations();
            c.OrderActionsBy(apiDesc => apiDesc.RelativePath);

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key needed to access the endpoints. Example: \"X-API-Key: {your_api_key}\"",
                Name = "X-API-Key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKey"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            c.DocumentFilter<SlugifyDocumentFilter>();
            c.OperationFilter<SwaggerCustomDescriptionOperationFilter>();
        });

        return services;
    }
}
