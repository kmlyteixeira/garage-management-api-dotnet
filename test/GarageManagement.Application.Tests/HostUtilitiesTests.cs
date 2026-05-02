using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using GarageManagement.Controllers;
using GarageManagement.Json;
using GarageManagement.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NSubstitute;
using Shouldly;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace GarageManagement;

public class HostUtilitiesTests
{
    [Fact]
    public void Branding_Provider_Should_Use_Localized_App_Name()
    {
        var localizer = Substitute.For<IStringLocalizer<GarageManagementResource>>();
        localizer["AppName"].Returns(new LocalizedString("AppName", "Garage Management"));

        var provider = new GarageManagementBrandingProvider(localizer);

        provider.AppName.ShouldBe("Garage Management");
    }

    [Fact]
    public void Home_Controller_Should_Redirect_To_Swagger()
    {
        var controller = new HomeController();

        var result = controller.Index() as RedirectResult;

        result.ShouldNotBeNull();
        result.Url.ShouldBe("~/swagger");
    }

    [Fact]
    public void Enum_Schema_Filter_Should_Convert_Descriptions()
    {
        var schema = new OpenApiSchema
        {
            Type = "integer",
            Format = "int32",
            Enum = new List<IOpenApiAny> { new OpenApiInteger(1) }
        };

        var filter = new EnumDescriptionSchemaFilter();
        var context = new SchemaFilterContext(typeof(TestEnum), null!, null!);

        filter.Apply(schema, context);

        schema.Type.ShouldBe("string");
        schema.Format.ShouldBeNull();
        schema.Enum.OfType<OpenApiString>().Any(item => item.Value == "First option").ShouldBeTrue();
        schema.Description.ShouldContain("First - First option");
        schema.Extensions.ShouldContainKey("x-enumDescriptions");
    }

    [Fact]
    public void Enum_Json_Converter_Factory_Should_Handle_Enum_And_Nullable_Enum()
    {
        var factory = new EnumDescriptionJsonConverterFactory();

        factory.CanConvert(typeof(TestEnum)).ShouldBeTrue();
        factory.CanConvert(typeof(TestEnum?)).ShouldBeTrue();
        factory.CanConvert(typeof(string)).ShouldBeFalse();

        var options = new JsonSerializerOptions();
        options.Converters.Add(factory);

        var json = JsonSerializer.Serialize(TestEnum.First, options);
        json.ShouldBe("\"First option\"");

        var parsed = JsonSerializer.Deserialize<TestEnum>("\"First option\"", options);
        parsed.ShouldBe(TestEnum.First);

        var nullableParsed = JsonSerializer.Deserialize<TestEnum?>("null", options);
        nullableParsed.ShouldBeNull();
    }

    private enum TestEnum
    {
        [Description("First option")]
        First,
        Second
    }
}