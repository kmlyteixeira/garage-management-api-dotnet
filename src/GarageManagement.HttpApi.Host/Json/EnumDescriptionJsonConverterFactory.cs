using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GarageManagement.Json;

public class EnumDescriptionJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        var enumType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
        return enumType.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var underlyingType = Nullable.GetUnderlyingType(typeToConvert);

        if (underlyingType == null)
        {
            var converterType = typeof(EnumDescriptionJsonConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        var nullableConverterType = typeof(NullableEnumDescriptionJsonConverter<>).MakeGenericType(underlyingType);
        return (JsonConverter)Activator.CreateInstance(nullableConverterType)!;
    }
}

public class EnumDescriptionJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    private readonly Dictionary<string, TEnum> parseMap;
    private readonly Dictionary<TEnum, string> writeMap;

    public EnumDescriptionJsonConverter()
    {
        parseMap = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);
        writeMap = new Dictionary<TEnum, string>();

        foreach (var value in Enum.GetValues<TEnum>())
        {
            var name = value.ToString();
            var description = GetDescription(value);

            parseMap[name] = value;
            parseMap[description] = value;
            writeMap[value] = description;
        }
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();
            return (TEnum)Enum.ToObject(typeof(TEnum), intValue);
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token {reader.TokenType} for enum {typeof(TEnum).Name}.");
        }

        var raw = reader.GetString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new JsonException($"Enum value for {typeof(TEnum).Name} cannot be empty.");
        }

        if (parseMap.TryGetValue(raw, out var parsed))
        {
            return parsed;
        }

        throw new JsonException($"Unable to convert '{raw}' to enum {typeof(TEnum).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (writeMap.TryGetValue(value, out var description))
        {
            writer.WriteStringValue(description);
            return;
        }

        writer.WriteStringValue(value.ToString());
    }

    private static string GetDescription(TEnum value)
    {
        var field = typeof(TEnum).GetField(value.ToString(), BindingFlags.Public | BindingFlags.Static);
        var description = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;
        return description ?? value.ToString();
    }
}

public class NullableEnumDescriptionJsonConverter<TEnum> : JsonConverter<TEnum?>
    where TEnum : struct, Enum
{
    private readonly EnumDescriptionJsonConverter<TEnum> innerConverter = new();

    public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return innerConverter.Read(ref reader, typeof(TEnum), options);
    }

    public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        innerConverter.Write(writer, value.Value, options);
    }
}
