using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Cyber
{
    /// <summary>
    /// 重复请求拦截辅助类
    /// 在 MiniAPI 生成的 handler 中调用
    /// </summary>
    public static class RepeatedRequestHelper
    {
        private static readonly CyberErrorResponseJsonConverter _errorResponseConverter = new();
        private static JsonTypeInfo<CyberErrorResponse>? _CyberErrorTypeInfo;
        private static JsonTypeInfo<CyberErrorResponse> CyberErrorTypeInfo
            => _CyberErrorTypeInfo ??= JsonMetadataServices.CreateValueInfo<CyberErrorResponse>(AppGlobalSettings.Option, _errorResponseConverter);

        /// <summary>
        /// MiniAPI 端点重复请求拦截入口
        /// 检查当前请求是否需要拦截，如果需要拦截则返回 true（已写入错误响应）
        /// 不需要拦截则设置缓存标记并返回 false
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns>是否为重复请求（已被拦截）</returns>
        public static async Task<bool> CheckAsync(HttpContext context)
        {
            var repeatedMethods = AppGlobalSettings.CyberSetting?.RepeatedRequestMethod;
            if (repeatedMethods == null || !repeatedMethods.Contains(context.Request.Method))
                return false;

            var cache = GetServiceProvider.GetScopeService<ICache>();
            if (cache == null)
                return false;

            // 计算参数哈希
            string paramHash = "";
            if (context.Request.HasJsonContentType())
            {
                try { context.Request.Body.Position = 0; paramHash = await new System.IO.StreamReader(context.Request.Body).ReadToEndAsync(); context.Request.Body.Position = 0; } catch { }
            }
            else
            {
                var qs = context.Request.QueryString.ToString();
                if (!string.IsNullOrEmpty(qs)) paramHash = qs;
            }

            var method = context.Request.Method;
            var url = context.Request.GetEncodedPathAndQuery();

            // 获取 JWT Token 用于标识用户（从 DI 获取默认 AuthOptions）
            string token = "";
            var authOptions = JwtAuthHelper.GetDefaultAuthOptions(context);
            if (authOptions != null)
            {
                token = JwtAuthHelper.GetTokenFromHeader(context, authOptions.HeaderName) ?? "";
            }

            // 计算请求唯一标识
            var raw = $"{method}{token}{url}{paramHash}";
            var md5Str = "Cyber:RepeatedRequest:" + ComputeMd5(raw);

            if (cache.TryGetValue(md5Str, out var cachedValue) && !string.IsNullOrEmpty(cachedValue))
            {
                // 重复请求，返回错误
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var errorResponse = new CyberErrorResponse
                {
                    Code = ResultCode.ParameterError,
                    Error = "Repeated Request",
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                await JsonSerializer.SerializeAsync(context.Response.Body, errorResponse, CyberErrorTypeInfo);
                return true;
            }

            // 非重复请求，存入缓存
            var expiration = AppGlobalSettings.CyberSetting?.RepeatedRequestCacheExpiration ?? 1800;
            cache.Set(md5Str, "1", expiration);
            context.Items["RequestMd5Flag"] = md5Str;
            return false;
        }

        /// <summary>
        /// 请求完成后清理重复请求缓存标记
        /// </summary>
        public static void ClearRequestFlag(HttpContext context)
        {
            if (context.Items.TryGetValue("RequestMd5Flag", out var md5Obj) && md5Obj is string md5Str)
            {
                var cache = GetServiceProvider.GetScopeService<ICache>();
                if (cache != null)
                {
                    cache.Del(md5Str);
                }
                context.Items.Remove("RequestMd5Flag");
            }
        }

        /// <summary>
        /// 计算 MD5 哈希
        /// </summary>
        private static string ComputeMd5(string input)
        {
            var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
