using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using JWT.Builder;

namespace JWT.Serializers
{
    /// <summary>
    /// AOT-safe <see cref="JsonConverter{T}"/> for <see cref="JwtHeader"/>.
    /// Honors the <c>JsonPropertyName</c> attributes on <see cref="JwtHeader"/> (e.g. <c>typ</c>,
    /// <c>alg</c>, <c>kid</c>) without relying on runtime reflection.
    /// </summary>
    internal sealed class JwtHeaderJsonConverter : JsonConverter<JwtHeader>
    {
        public override JwtHeader Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null!;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected StartObject token.");

            var header = new JwtHeader();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return header;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected PropertyName token.");

                var propertyName = reader.GetString();
                if (String.IsNullOrEmpty(propertyName))
                    throw new JsonException("Property name is empty.");

                reader.Read();

                switch (propertyName)
                {
                    case "typ":
                        header.Type = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                        break;
                    case "cty":
                        header.ContentType = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                        break;
                    case "alg":
                        header.Algorithm = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                        break;
                    case "kid":
                        header.KeyId = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                        break;
                    case "x5u":
                        header.X5u = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                        break;
                    case "x5t":
                        header.X5t = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                        break;
                    case "x5c":
                        if (reader.TokenType == JsonTokenType.Null)
                        {
                            header.X5c = null;
                        }
                        else
                        {
                            if (reader.TokenType != JsonTokenType.StartArray)
                                throw new JsonException("Expected StartArray token for x5c.");

                            var list = new System.Collections.Generic.List<string>();
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            {
                                list.Add(reader.GetString()!);
                            }
                            header.X5c = list.ToArray();
                        }
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonException("Unexpected end of JSON.");
        }

        public override void Write(Utf8JsonWriter writer, JwtHeader value, JsonSerializerOptions options)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteStartObject();

            if (value is null)
            {
                writer.WriteEndObject();
                return;
            }

            if (value.Type != null) writer.WriteString("typ", value.Type);
            if (value.ContentType != null) writer.WriteString("cty", value.ContentType);
            if (value.Algorithm != null) writer.WriteString("alg", value.Algorithm);
            if (value.KeyId != null) writer.WriteString("kid", value.KeyId);
            if (value.X5u != null) writer.WriteString("x5u", value.X5u);
            if (value.X5t != null) writer.WriteString("x5t", value.X5t);

            if (value.X5c != null)
            {
                writer.WriteStartArray("x5c");
                foreach (var cert in value.X5c)
                {
                    writer.WriteStringValue(cert);
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}
