using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cyber
{
    /// <summary>
    /// CyberErrorResponse 的 JsonConverter
    /// </summary>
    public class CyberErrorResponseJsonConverter : JsonConverter<CyberErrorResponse>
    {
        public override CyberErrorResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return null;
        }

        public override void Write(Utf8JsonWriter writer, CyberErrorResponse value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("Code", (int)value.Code);
            
            if (value.Error != null)
                writer.WriteString("Error", value.Error);
            else
                writer.WriteString("Error", "");

            if (value.Timestamp.HasValue)
                writer.WriteNumber("Timestamp", value.Timestamp.Value);
            else
                writer.WriteNumber("Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            writer.WriteEndObject();
        }
    }

    public class CyberErrorsResponseJsonConverter : JsonConverter<CyberErrorsResponse>
    {
        public override CyberErrorsResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return null;
        }

        public override void Write(Utf8JsonWriter writer, CyberErrorsResponse value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("Code", (int)value.Code);

            writer.WritePropertyName("Error");
            if (value.Error != null && value.Error.Length > 0)
            {
                writer.WriteStartArray();
                foreach (var msg in value.Error)
                {
                    writer.WriteStringValue(msg);
                }
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
            }

            if (value.Timestamp.HasValue)
                writer.WriteNumber("Timestamp", value.Timestamp.Value);
            else
                writer.WriteNumber("Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            writer.WriteEndObject();
        }
    }

}