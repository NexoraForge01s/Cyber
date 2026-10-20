using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Cyber.AOT.Common
{
    //因为jsontype 由source 生成 所以不需要这个 GetTypeInfo为null即可
    public sealed class CyberJsonTypeInfoResolver : IJsonTypeInfoResolver
    {
        public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            return null;
        }
    }
}
