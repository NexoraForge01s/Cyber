using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using JWT.Serializers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Cyber
{
    /// <summary>
    /// JWT 鉴权辅助类
    /// 提供 MiniAPI 端点的 JWT 验证和续期功能
    /// </summary>
    public static class JwtAuthHelper
    {
        private static readonly CyberErrorResponseJsonConverter _errorResponseConverter = new();
        private static JsonTypeInfo<CyberErrorResponse>? _CyberErrorTypeInfo;
        private static JsonTypeInfo<CyberErrorResponse> CyberErrorTypeInfo
            => _CyberErrorTypeInfo ??= JsonMetadataServices.CreateValueInfo<CyberErrorResponse>(AppGlobalSettings.Option, _errorResponseConverter);

        /// <summary>
        /// 从请求中统一获取 JWT Token
        /// 优先级：指定Header -> Authorization Header -> X-Token Header -> Query Token -> Cookie Token
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="headerName">Token 所在的 Header 名称，空字符串则使用统一获取逻辑</param>
        /// <returns>Token 字符串，未找到返回 null</returns>
        public static string? GetTokenFromHeader(HttpContext context, string headerName)
        {
            return !headerName.IsNullOrEmpty()
                ? context.Request.Headers[headerName].FirstOrDefault()?.Split(" ").Last()
                : context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last()
                  ?? context.Request.Headers["X-Token"].FirstOrDefault()
                  ?? context.Request.Query["Token"].FirstOrDefault()
                  ?? context.Request.Cookies["Token"];
        }

        /// <summary>
        /// 验证 JWT Token 并返回是否有效
        /// </summary>
        public static JwtValidationResult ValidateToken(string token, string security)
        {
            try
            {
                var payload = JwtBuilder.Create()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(security)
                    .MustVerifySignature()
                    .Decode<IDictionary<string, object>>(token, JsonTypeInfoShared.IDictTypeInfo);

                return new JwtValidationResult { IsValid = true, Payload = payload };
            }
            catch (TokenExpiredException)
            {
                return new JwtValidationResult { IsValid = false, Error = "Token expired" };
            }
            catch (SignatureVerificationException)
            {
                return new JwtValidationResult { IsValid = false, Error = "Invalid token signature" };
            }
            catch (Exception ex)
            {
                return new JwtValidationResult { IsValid = false, Error = ex.Message };
            }
        }

        /// <summary>
        /// 续期 JWT Token：保持原有 payload，更新过期时间
        /// </summary>
        public static string RenewToken(IDictionary<string, object> payload, string security, double expirationSeconds)
        {
            var now = DateTime.UtcNow;
            var newExp = UnixEpoch.GetSecondsSince(now.AddSeconds(expirationSeconds));
            var newIat = UnixEpoch.GetSecondsSince(now);

            if (payload.ContainsKey("exp"))
                payload["exp"] = newExp;
            else
                payload.Add("exp", newExp);

            if (payload.ContainsKey("iat"))
                payload["iat"] = newIat;
            else
                payload.Add("iat", newIat);

            return JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(security)
                .Encode(payload, JsonTypeInfoShared.IDictTypeInfo);
        }

        /// <summary>
        /// 从 DI 容器获取默认 AuthOptions（Config 为 null 的那个）
        /// </summary>
        public static AuthOptions? GetDefaultAuthOptions(HttpContext context)
        {
            return context.RequestServices.GetService<AuthOptions>();
        }

        /// <summary>
        /// MiniAPI 端点鉴权入口：从 DI 获取默认 AuthOptions 并执行鉴权
        /// 鉴权通过返回 true，失败返回 false（已写入错误响应）
        /// 无默认 AuthOptions 时返回 true（未配置JWT则跳过）
        /// </summary>
        public static async Task<bool> AuthenticateAsync(HttpContext context)
        {
            var authOptions = GetDefaultAuthOptions(context);
            if (authOptions == null)
                return true;

            var token = GetTokenFromHeader(context, authOptions.HeaderName);
            if (string.IsNullOrEmpty(token))
            {
                await WriteAuthErrorAsync(context, authOptions, authOptions.ErrorMessage);
                return false;
            }

            var result = ValidateToken(token, authOptions.Security);
            if (!result.IsValid)
            {
                await WriteAuthErrorAsync(context, authOptions, authOptions.ErrorMessage);
                return false;
            }

            // JWT 有效，续期并在响应头添加 XSS-Authorization
            try
            {
                var newToken = RenewToken(result.Payload!, authOptions.Security, authOptions.Expiration);
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["XSS-Authorization"] = newToken;
                    return Task.CompletedTask;
                });
            }
            catch
            {
                // 续期失败不影响正常请求
            }

            return true;
        }

        /// <summary>
        /// 写入鉴权错误响应
        /// </summary>
        private static async Task WriteAuthErrorAsync(HttpContext context, AuthOptions authOptions, string errorMessage)
        {
            if (authOptions.AuthOptionType == AuthOptionEnum.Action && authOptions.Action != null)
            {
                authOptions.Action(context, errorMessage);
                return;
            }

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var errorResponse = new CyberErrorResponse
            {
                Code = ResultCode.NeedLogin,
                Error = errorMessage
            };
            await JsonSerializer.SerializeAsync(context.Response.Body, errorResponse, CyberErrorTypeInfo);
        }
    }

    /// <summary>
    /// JWT 验证结果
    /// </summary>
    public class JwtValidationResult
    {
        public bool IsValid { get; set; }
        public IDictionary<string, object>? Payload { get; set; }
        public string? Error { get; set; }
    }
}
