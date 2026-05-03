using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace GarageManagement;

public class SwaggerTagDescriptionFilter : IDocumentFilter
{
    private static readonly Dictionary<string, string> TagDescriptions = new()
    {
        {
            "Customer",
            "Endpoints para gerenciar clientes."
        },
        {
            "Estimate",
            "Endpoints para gerenciar orçamentos."
        },
        {
            "Service",
            "Endpoints para gerenciar serviços disponíveis."
        },
        {
            "Product",
            "Endpoints para gerenciar peças/insumos."
        },
        {
            "Inventory",
            "Endpoints para gerenciar estoque de produtos."
        },
        {
            "ServiceOrder",
            "Endpoints para gerenciar ordens de serviço."
        },
        {
            "Vehicle",
            "Endpoints para gerenciar veículos dos clientes."
        },
        {
            "ServiceExecutionMonitoring",
            "Endpoints para monitorar o tempo de execução dos serviços."
        }
    };

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc.Tags == null)
            swaggerDoc.Tags = new List<OpenApiTag>();

        foreach (var path in swaggerDoc.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                foreach (var tag in operation.Tags)
                {
                    if (TagDescriptions.TryGetValue(tag.Name, out var description))
                    {
                        tag.Description = description;
                    }
                }
            }
        }

        foreach (var tagKey in TagDescriptions.Keys)
        {
            var existingTag = swaggerDoc.Tags.FirstOrDefault(t => t.Name == tagKey);
            if (existingTag != null)
            {
                existingTag.Description = TagDescriptions[tagKey];
            }
            else
            {
                swaggerDoc.Tags.Add(new OpenApiTag
                {
                    Name = tagKey,
                    Description = TagDescriptions[tagKey]
                });
            }
        }
    }
}
