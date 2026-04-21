using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GarageManagement;

public class EnumDescriptionSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var enumType = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
        if (!enumType.IsEnum)
        {
            return;
        }

        schema.Type = "string";
        schema.Format = null;
        schema.Enum.Clear();

        var names = Enum.GetNames(enumType);
        var descriptions = names.Select(name =>
        {
            var field = enumType.GetField(name, BindingFlags.Public | BindingFlags.Static);
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            var description = attribute?.Description ?? name;
            return new
            {
                Name = name,
                Description = description
            };
        }).ToList();

        foreach (var item in descriptions)
        {
            schema.Enum.Add(new OpenApiString(item.Description));
        }

        schema.Description = string.Join(Environment.NewLine, descriptions.Select(x => $"{x.Name} - {x.Description}"));

        var extension = new OpenApiArray();
        foreach (var item in descriptions)
        {
            extension.Add(new OpenApiString($"{item.Name} - {item.Description}"));
        }

        schema.Extensions["x-enumDescriptions"] = extension;
    }
}
