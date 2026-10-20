using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Cyber
{
    /// <summary>
    /// MiniAPI 异常处理辅助类
    /// 封装异常响应序列化逻辑，减少生成代码重复
    /// </summary>
    public static class MiniApiExceptionHelper
    {
        /// <summary>
        /// 处理 NeedLoginException 异常响应
        /// 单个错误返回 CyberErrorResponse，多个错误返回 CyberErrorsResponse
        /// </summary>
        public static async Task HandleNeedLoginExceptionAsync(
            HttpContext ctx,
            NeedLoginException ex)
        {
            ctx.Response.StatusCode = 401;
            ctx.Response.ContentType = "application/json";

            if (ex._ArrayData != null && ex._ArrayData.Count > 0)
            {
                if (ex._ArrayData.Count == 1)
                {
                    await JsonSerializer.SerializeAsync(ctx.Response.Body,
                        new CyberErrorResponse
                        {
                            Code = ResultCode.Exception,
                            Error = ex._ArrayData[0],
                            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        },
                        JsonTypeInfoShared.CyberErrorResponse);
                }
                else
                {
                    await JsonSerializer.SerializeAsync(ctx.Response.Body,
                        new CyberErrorsResponse
                        {
                            Code = ResultCode.Exception,
                            Error = ex._ArrayData.ToArray(),
                            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        },
                        JsonTypeInfoShared.CyberErrorsResponse);
                }
            }
            else
            {
                await JsonSerializer.SerializeAsync(ctx.Response.Body,
                    new CyberErrorResponse
                    {
                        Code = ResultCode.Exception,
                        Error = ex.Message,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    },
                    JsonTypeInfoShared.CyberErrorResponse);
            }
        }

        /// <summary>
        /// 处理 OutPutException 异常响应
        /// 单个错误返回 CyberErrorResponse，多个错误返回 CyberErrorsResponse
        /// </summary>
        public static async Task HandleOutPutExceptionAsync(
            HttpContext ctx,
            OutPutException ex)
        {
            ctx.Response.StatusCode = 500;
            ctx.Response.ContentType = "application/json";

            if (ex._ArrayData != null && ex._ArrayData.Length > 0)
            {
                if (ex._ArrayData.Length == 1)
                {
                    await JsonSerializer.SerializeAsync(ctx.Response.Body,
                        new CyberErrorResponse
                        {
                            Code = ResultCode.ParameterError,
                            Error = ex._ArrayData[0],
                            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        },
                        JsonTypeInfoShared.CyberErrorResponse);
                }
                else
                {
                    await JsonSerializer.SerializeAsync(ctx.Response.Body,
                        new CyberErrorsResponse
                        {
                            Code = ResultCode.ParameterError,
                            Error = ex._ArrayData,
                            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        },
                        JsonTypeInfoShared.CyberErrorsResponse);
                }
            }
            else
            {
                await JsonSerializer.SerializeAsync(ctx.Response.Body,
                    new CyberErrorResponse
                    {
                        Code = ResultCode.ParameterError,
                        Error = ex.Message,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    },
                    JsonTypeInfoShared.CyberErrorResponse);
            }
        }

        /// <summary>
        /// 处理通用 Exception 异常响应
        /// </summary>
        public static async Task HandleGenericExceptionAsync(
            HttpContext ctx,
            Exception ex)
        {
            ctx.Response.StatusCode = 500;
            ctx.Response.ContentType = "application/json";

            await JsonSerializer.SerializeAsync(ctx.Response.Body,
                new CyberErrorResponse
                {
                    Code = ResultCode.Exception,
                    Error = ex.Message,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                },
               JsonTypeInfoShared.CyberErrorResponse);
        }

        /// <summary>
        /// 处理验证结果，如果验证失败则返回错误响应
        /// 单个错误返回 CyberErrorResponse，多个错误返回 CyberErrorsResponse
        /// </summary>
        /// <returns>验证是否通过</returns>
        public static async Task<bool> HandleValidationResultAsync(
            HttpContext ctx,
            ValidationResult validationResult)
        {
            if (validationResult.IsValid)
            {
                return true;
            }

            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();

            if (errors.Length == 1)
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(ctx.Response.Body,
                    new CyberErrorResponse
                    {
                        Code = ResultCode.Exception,
                        Error = errors[0],
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    },
                    JsonTypeInfoShared.CyberErrorResponse);
            }
            else
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(ctx.Response.Body,
                    new CyberErrorsResponse
                    {
                        Code = ResultCode.Exception,
                        Error = errors,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    },
                    JsonTypeInfoShared.CyberErrorsResponse);
            }

            return false;
        }

        /// <summary>
        /// 从请求体读取 JSON 并验证，如果失败则返回错误响应
        /// 包含：JSON 解析异常处理、null 检查
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="ctx">HttpContext</param>
        /// <param name="typeInfo">JsonTypeInfo</param>
        /// <param name="parameterName">参数名称（用于错误消息）</param>
        /// <param name="errorResponseTypeInfo">错误响应的 JsonTypeInfo</param>
        /// <returns>读取成功返回对象，失败返回 null</returns>
        public static async Task<T?> ReadJsonAsync<T>(
            HttpContext ctx,
            JsonTypeInfo<T> typeInfo,
            string parameterName) where T : class
        {
            try
            {
                var result = await ctx.Request.ReadFromJsonAsync(typeInfo);

                if (result == null)
                {
                    ctx.Response.StatusCode = 500;
                    ctx.Response.ContentType = "application/json";
                    await JsonSerializer.SerializeAsync(ctx.Response.Body,
                        new CyberErrorResponse
                        {
                            Code = ResultCode.ParameterError,
                            Error = $"Null JSON body for parameter: {parameterName}",
                            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        },
                        JsonTypeInfoShared.CyberErrorResponse);
                    return null;
                }

                return result;
            }
            catch (JsonException)
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(ctx.Response.Body,
                    new CyberErrorResponse
                    {
                        Code = ResultCode.ParameterError,
                        Error = "Invalid request parameter",
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    },
                     JsonTypeInfoShared.CyberErrorResponse);
                return null;
            }
        }
    }
}