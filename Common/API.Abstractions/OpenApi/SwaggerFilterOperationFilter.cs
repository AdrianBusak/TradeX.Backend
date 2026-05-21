using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection;

namespace API.Abstractions.OpenApi;

public class SwaggerCustomDescriptionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            return;

        foreach (var param in operation.Parameters)
        {
            var methodParam = context.MethodInfo.GetParameters()
                .FirstOrDefault(p => p.Name == param.Name);

            if (methodParam == null)
                continue;

            var swaggerAttr = methodParam.GetCustomAttributes()
                .OfType<SwaggerParameterAttribute>()
                .FirstOrDefault();

            if (swaggerAttr != null)
            {
                param.Description = swaggerAttr.Description;
                //param.Required = swaggerAttr.Required;
            }
        }
    }
}
