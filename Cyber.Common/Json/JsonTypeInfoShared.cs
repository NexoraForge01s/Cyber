using JWT.Builder;
using JWT.Serializers;
using Cyber.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace Cyber
{
    public static class JsonTypeInfoShared
    {
        private static readonly CyberErrorResponseJsonConverter CyberErrorResponseConverter = new();
        private static readonly CyberErrorsResponseJsonConverter CyberErrorsResponseConverter = new();
        private static readonly JwtHeaderJsonConverter _jwtHeaderConverter = new();
        private static readonly DictionaryStringObjectJsonConverter _dictConverter = new();
        private static readonly IDictionaryStringObjectJsonConverter _idictConverter = new();


        private static JsonTypeInfo<Cyber.CyberErrorResponse>? _CyberErrorResponse;
        private static JsonTypeInfo<Cyber.CyberErrorsResponse>? _CyberErrorsResponse;
        private static JsonTypeInfo<JwtHeader>? _jwtHeaderTypeInfo;
        private static JsonTypeInfo<Dictionary<string, object>>? _dictTypeInfo;
        private static JsonTypeInfo<IDictionary<string, object>>? _idictTypeInfo;

        public static JsonTypeInfo<Cyber.CyberErrorResponse> CyberErrorResponse
         => _CyberErrorResponse ??= JsonMetadataServices.CreateValueInfo<Cyber.CyberErrorResponse>(AppGlobalSettings.Option, CyberErrorResponseConverter);

        public static JsonTypeInfo<Cyber.CyberErrorsResponse> CyberErrorsResponse
            => _CyberErrorsResponse ??= JsonMetadataServices.CreateValueInfo<Cyber.CyberErrorsResponse>(AppGlobalSettings.Option, CyberErrorsResponseConverter);

        public static JsonTypeInfo<JwtHeader> JwtHeaderTypeInfo
         => _jwtHeaderTypeInfo ??= JsonMetadataServices.CreateValueInfo<JwtHeader>(AppGlobalSettings.Option, _jwtHeaderConverter);

        /// <summary>Cached <see cref="JsonTypeInfo{T}" /> for <see cref="Dictionary{TKey,TValue}" />.</summary>
        public static JsonTypeInfo<Dictionary<string, object>> DictTypeInfo
            => _dictTypeInfo ??= JsonMetadataServices.CreateValueInfo<Dictionary<string, object>>(AppGlobalSettings.Option, _dictConverter);

        /// <summary>Cached <see cref="JsonTypeInfo{T}" /> for <see cref="IDictionary{TKey,TValue}" />.</summary>
        public static JsonTypeInfo<IDictionary<string, object>> IDictTypeInfo
            => _idictTypeInfo ??= JsonMetadataServices.CreateValueInfo<IDictionary<string, object>>(AppGlobalSettings.Option, _idictConverter);
    }
}
