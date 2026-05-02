using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using GarageManagement.Json;
using Shouldly;
using Xunit;

namespace GarageManagement.Json.Tests;

public class EnumDescriptionJsonConverterFactoryTests
{
    private enum TestEnum
    {
        [Description("First description")]
        First = 0,
        Second = 1
    }

    [Fact]
    public void CanConvert_Should_Return_True_For_Enum_And_Nullable()
    {
        var factory = new EnumDescriptionJsonConverterFactory();

        factory.CanConvert(typeof(TestEnum)).ShouldBeTrue();
        factory.CanConvert(typeof(TestEnum?)).ShouldBeTrue();
        factory.CanConvert(typeof(string)).ShouldBeFalse();
    }

    [Fact]
    public void CreateConverter_Should_Return_Expected_Concrete_Types()
    {
        var factory = new EnumDescriptionJsonConverterFactory();

        var nonNullable = factory.CreateConverter(typeof(TestEnum), new JsonSerializerOptions());
        nonNullable.ShouldNotBeNull();
        nonNullable.GetType().Name.ShouldStartWith("EnumDescriptionJsonConverter");

        var nullable = factory.CreateConverter(typeof(TestEnum?), new JsonSerializerOptions());
        nullable.ShouldNotBeNull();
        nullable.GetType().Name.ShouldStartWith("NullableEnumDescriptionJsonConverter");
    }

    [Fact]
    public void EnumConverter_Read_Number_And_String_And_Invalid()
    {
        var converter = new EnumDescriptionJsonConverter<TestEnum>();

        // number
        var bytes = Encoding.UTF8.GetBytes("1");
        var reader = new Utf8JsonReader(bytes);
        reader.Read();
        var value = converter.Read(ref reader, typeof(TestEnum), new JsonSerializerOptions());
        value.ShouldBe(TestEnum.Second);

        // known description string
        var json = "\"First description\"";
        var bytes2 = Encoding.UTF8.GetBytes(json);
        var reader2 = new Utf8JsonReader(bytes2);
        reader2.Read();
        var value2 = converter.Read(ref reader2, typeof(TestEnum), new JsonSerializerOptions());
        value2.ShouldBe(TestEnum.First);

        // unknown string
        Should.Throw<JsonException>(() => ReadUnknownValue(converter));
    }

    [Fact]
    public void NullableEnumConverter_Read_Null_Returns_Null()
    {
        var converter = new NullableEnumDescriptionJsonConverter<TestEnum>();

        var bytes = Encoding.UTF8.GetBytes("null");
        var reader = new Utf8JsonReader(bytes);
        reader.Read();

        var value = converter.Read(ref reader, typeof(TestEnum?), new JsonSerializerOptions());

        value.ShouldBeNull();
    }

    [Fact]
    public void NullableEnumConverter_Write_Null_Writes_Json_Null()
    {
        var converter = new NullableEnumDescriptionJsonConverter<TestEnum>();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        converter.Write(writer, null, new JsonSerializerOptions());
        writer.Flush();

        Encoding.UTF8.GetString(stream.ToArray()).ShouldBe("null");
    }

    private static void ReadUnknownValue(EnumDescriptionJsonConverter<TestEnum> converter)
    {
        var bad = Encoding.UTF8.GetBytes("\"unknown\"");
        var reader = new Utf8JsonReader(bad);
        reader.Read();
        converter.Read(ref reader, typeof(TestEnum), new JsonSerializerOptions());
    }
}
