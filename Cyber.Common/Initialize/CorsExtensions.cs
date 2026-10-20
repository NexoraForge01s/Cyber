using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cyber
{
    public static class CorsExtensions
    {
        /// <summary>
        /// 注册 CORS 跨域服务（根据 CyberSetting.CorsModules 配置）
        /// </summary>
        public static IServiceCollection AddCorsExtensions(this IServiceCollection services)
        {
            var corsModules = AppGlobalSettings.CyberSetting.CorsModules;

            if (corsModules != null && corsModules.Count > 0)
            {
                var policyNames = new List<string>();
                int i = 0;
                foreach (var corsModule in corsModules)
                {
                    string name = corsModule.Name.IsNullOrEmpty() ? $"CorsPolicy{i}" : corsModule.Name;
                    policyNames.Add(name);

                    services.AddCors(options =>
                    {
                        options.AddPolicy(name, builder =>
                        {
                            if (corsModule.Origin != null && corsModule.Origin.Length > 0)
                                builder.WithOrigins(corsModule.Origin).SetIsOriginAllowedToAllowWildcardSubdomains();
                            else
                                builder.AllowAnyOrigin();

                            if (corsModule.Headers != null && corsModule.Headers.Length == 1 && corsModule.Headers[0] == "*")
                                builder.AllowAnyHeader();
                            else if (corsModule.Headers != null && corsModule.Headers.Length > 0)
                                builder.WithHeaders(corsModule.Headers);
                            else
                                builder.AllowAnyHeader();

                            if (corsModule.Methods != null && corsModule.Methods.Length == 1 && corsModule.Methods[0] == "*")
                                builder.AllowAnyMethod();
                            else if (corsModule.Methods != null && corsModule.Methods.Length > 0)
                                builder.WithMethods(corsModule.Methods);
                            else
                                builder.AllowAnyMethod();

                            if (corsModule.AllowCredentials)
                                builder.AllowCredentials();

                            if (corsModule.WithExposedHeaders != null && corsModule.WithExposedHeaders.Length > 0)
                                builder.WithExposedHeaders(corsModule.WithExposedHeaders);
                        });
                    });

                    i++;
                }

                AppGlobalSettings.CyberSetting.CorsPolicyNames = policyNames;
                AppGlobalSettings.CyberSetting.CorsEnabled = true;

                services.AddResponseCaching();
            }

            return services;
        }
    }
}
