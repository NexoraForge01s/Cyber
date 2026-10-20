using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Cyber.AOT.Common.Middleware
{
    /// <summary>
    /// Cyber状态码统一处理中间件（404/405等）
    /// </summary>
    public class CyberStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public CyberStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // 处理404 - 未找到端点
            if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
            {
                var errorResponse = new CyberErrorResponse
                {
                    Code = ResultCode.NoFound,
                    Error = "Endpoint not found",
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                await WriteErrorResponse(context, 404, errorResponse);
            }

            // 处理405 - 方法不允许
            if (context.Response.StatusCode == 405 && !context.Response.HasStarted)
            {
                var errorResponse = new CyberErrorResponse
                {
                    Code = ResultCode.NoMethod,
                    Error = "Method not allowed",
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                await WriteErrorResponse(context, 405, errorResponse);
            }
        }

        private static async Task WriteErrorResponse(HttpContext context, int statusCode, CyberErrorResponse errorResponse)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await JsonSerializer.SerializeAsync(context.Response.Body, errorResponse, JsonTypeInfoShared.CyberErrorResponse);

        }
    }
}
