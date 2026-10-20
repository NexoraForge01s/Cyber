using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Cyber.Json
{
    internal static class DictionaryStringObjectConverterCore
    {
        public static IDictionary<string, object?> Read(
            ref Utf8JsonReader reader,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected StartObject.");

            IDictionary<string, object?> dictionary =
                new Dictionary<string, object?>(StringComparer.Ordinal);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return dictionary;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string? propertyName = reader.GetString();

                if (propertyName == null)
                    throw new JsonException();

                reader.Read();

                dictionary[propertyName] = ReadValue(ref reader, options);
            }

            throw new JsonException();
        }

        private static object? ReadValue(
            ref Utf8JsonReader reader,
            JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:

                    if (reader.TryGetDateTime(out var dt))
                        return dt;

                    if (reader.TryGetGuid(out var guid))
                        return guid;

                    return reader.GetString();

                case JsonTokenType.True:
                    return true;

                case JsonTokenType.False:
                    return false;

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.Number:

                    if (reader.TryGetInt32(out var i))
                        return i;

                    if (reader.TryGetInt64(out var l))
                        return l;

                    if (reader.TryGetDecimal(out var dec))
                        return dec;

                    return reader.GetDouble();

                case JsonTokenType.StartObject:

                    return Read(ref reader, options);

                case JsonTokenType.StartArray:

                    var list = new List<object?>();

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                            return list;

                        list.Add(ReadValue(ref reader, options));
                    }

                    throw new JsonException();

                default:
                    throw new JsonException($"Unsupported token {reader.TokenType}");
            }
        }

        public static void Write(
            Utf8JsonWriter writer,
            IDictionary<string, object?> dictionary,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kv in dictionary)
            {
                writer.WritePropertyName(kv.Key);
                WriteValue(writer, kv.Value, options);
            }

            writer.WriteEndObject();
        }

        private static void WriteValue(
            Utf8JsonWriter writer,
            object? value,
            JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            switch (value)
            {
                case string s:
                    writer.WriteStringValue(s);
                    return;

                case bool b:
                    writer.WriteBooleanValue(b);
                    return;

                case byte n:
                    writer.WriteNumberValue(n);
                    return;

                case sbyte n:
                    writer.WriteNumberValue(n);
                    return;

                case short n:
                    writer.WriteNumberValue(n);
                    return;

                case ushort n:
                    writer.WriteNumberValue(n);
                    return;

                case int n:
                    writer.WriteNumberValue(n);
                    return;

                case uint n:
                    writer.WriteNumberValue(n);
                    return;

                case long n:
                    writer.WriteNumberValue(n);
                    return;

                case ulong n:
                    writer.WriteNumberValue(n);
                    return;

                case float n:
                    writer.WriteNumberValue(n);
                    return;

                case double n:
                    writer.WriteNumberValue(n);
                    return;

                case decimal n:
                    writer.WriteNumberValue(n);
                    return;

                case DateTime dt:
                    writer.WriteStringValue(dt);
                    return;

                case DateTimeOffset dto:
                    writer.WriteStringValue(dto);
                    return;

                case Guid guid:
                    writer.WriteStringValue(guid);
                    return;

                case IDictionary<string, object> dict:
                    {
                        writer.WriteStartObject();

                        foreach (var kv in dict)
                        {
                            writer.WritePropertyName(kv.Key);
                            WriteValue(writer, kv.Value, options);
                        }

                        writer.WriteEndObject();
                        return;
                    }

                case IEnumerable enumerable when value is not string:

                    writer.WriteStartArray();

                    foreach (var item in enumerable)
                    {
                        WriteValue(writer, item, options);
                    }

                    writer.WriteEndArray();
                    return;

                default:

                    JsonTypeInfo typeInfo = options.GetTypeInfo(value.GetType());

                    JsonSerializer.Serialize(
                        writer,
                        value,
                        typeInfo);

                    return;
            }
        }
    }

    /// <summary>
    /// Converter for Dictionary&lt;string,object&gt;
    /// </summary>
    public sealed class DictionaryStringObjectJsonConverter
        : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return (Dictionary<string, object>)
                DictionaryStringObjectConverterCore.Read(ref reader, options);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<string, object> value,
            JsonSerializerOptions options)
        {
            DictionaryStringObjectConverterCore.Write(writer, value, options);
        }
    }

    /// <summary>
    /// Converter for IDictionary&lt;string,object&gt;
    /// </summary>
    public sealed class IDictionaryStringObjectJsonConverter
        : JsonConverter<IDictionary<string, object>>
    {
        public override IDictionary<string, object> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return DictionaryStringObjectConverterCore.Read(ref reader, options);
        }

        public override void Write(
            Utf8JsonWriter writer,
            IDictionary<string, object> value,
            JsonSerializerOptions options)
        {
            DictionaryStringObjectConverterCore.Write(writer, value, options);
        }
    }
}