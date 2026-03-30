using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PizzaStore.API.Filters;

/// <summary>
/// Schema filter to display enums as strings in Swagger UI
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Type = "string";
            schema.Enum.Clear();

            foreach (var enumValue in Enum.GetValues(context.Type))
            {
                var enumName = enumValue.ToString();
                if (!string.IsNullOrEmpty(enumName))
                {
                    schema.Enum.Add(new OpenApiString(enumName));
                }
            }
        }
    }
}
