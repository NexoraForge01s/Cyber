using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Cyber;
using Cyber.AOT.Common;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Cyber
{
    public class CyberSetting
    {
        private readonly Dictionary<Type, object> _options = new();

        /// <summary>
        /// 返回的日期格式
        /// </summary>
        public string JsonDateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// 验证语言请求头名称
        /// </summary>
        public string ValidationLanguageHeaderName { get; set; } = "ValidationLanguage";

        /// <summary>
        /// 日志组件的实例 如果为空 则为默认配置
        /// </summary>
        public ILog ILog { get; set; }

        /// <summary>
        /// 日志配置方法
        /// </summary>
        public void LogConfiguration(Action<LogSetting> logConfiguration)
        {
            var logSetting = new LogSetting();
            logConfiguration(logSetting);
            _options.Add(typeof(LogSetting), logSetting);
        }

        public void ConfigureServices(Action<IServiceCollection> action)
        {

            var actions= GetOption<List<Action<IServiceCollection>>>();
            if(actions==null)
                actions=new List<Action<IServiceCollection>>();

            actions.Add(action);
            _options.AddOrUpdate(typeof(List<Action<IServiceCollection>>), actions);
        }

        public void JsonSerializerOptions(Action<JsonSerializerOptions> action)
        {
            action(AppGlobalSettings.Option);
        }


        /// <summary>
        /// 获取配置选项
        /// </summary>
        public T? GetOption<T>() where T : class, new()
        {
            var type = typeof(T);

            if (_options.TryGetValue(type, out var value))
            {
                return (T)value;
            }
            else
            {
                return null;
            }
        }

        public LogSwitch LogSwitch { get; set; } = new LogSwitch();

        #region Cors 配置

        /// <summary>
        /// CORS 跨域配置列表
        /// </summary>
        public List<CorsModule> CorsModules { get; set; } = new List<CorsModule>();

        /// <summary>
        /// CORS 策略名称列表（由 CorsExtensions 自动填充）
        /// </summary>
        internal List<string> CorsPolicyNames { get; set; } = new List<string>();

        /// <summary>
        /// CORS 是否已启用（由 CorsExtensions 自动设置）
        /// </summary>
        public bool CorsEnabled { get; set; }

        #endregion

        #region JWT 鉴权配置

        /// <summary>
        /// JWT认证忽略的特性类型列表
        /// 默认包含 AllowAnonymousAttribute
        /// </summary>
        public List<Type> JwtIgnoreAttributes { get; set; } = new List<Type> { typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute) };

        /// <summary>
        /// 开启JWT集成验证（单个配置）
        /// </summary>
        public void QuickJWT(Action<AuthOptions> quickJWT)
        {
            var list = GetOption<List<Action<AuthOptions>>>();
            if (list == null)
            {
                list = new List<Action<AuthOptions>>();
                _options.AddOrUpdate(typeof(List<Action<AuthOptions>>), list);
            }
            list.Add(quickJWT);
        }

        #endregion

        #region 重复请求拦截配置

        /// <summary>
        /// 需要进行重复请求拦截的 HTTP 方法列表
        /// 默认拦截 POST、PUT、DELETE
        /// </summary>
        public List<string> RepeatedRequestMethod { get; set; } = new List<string> { "POST", "PUT", "DELETE" };

        /// <summary>
        /// 重复请求缓存过期时间（秒），默认1800秒
        /// </summary>
        public int RepeatedRequestCacheExpiration { get; set; } = 1800;

        #endregion
    }



    /// <summary>
    /// 日志配置设置
    /// </summary>
    public class LogSetting
    {
        /// <summary>
        /// 自定义 LoggerConfiguration 配置方法
        /// </summary>
        public Action<Serilog.LoggerConfiguration> CustomLoggerConfiguration { get; set; }

        /// <summary>
        /// 自定义 File 写入配置方法
        /// 参数1: options - File配置选项(已包含默认值)
        /// </summary>
        public Action<Cyber.Sinks.File.FileLoggerOptions> CustomFileConfiguration { get; set; }
    }

    public class LogSwitch
    {
        public bool Debug { get; set; } = true;

        public bool Information { get; set; } = true;

        public bool Warning { get; set; } = true;

        public bool Error { get; set; } = true;

        public bool Sql { get; set; } = true;

        public bool Operate { get; set; } = true;
    }

    public static class AppGlobalSettings
    {
        public static CyberSetting CyberSetting { get; set; } = new CyberSetting();
        public static RunTimeEunm RunTimeEunm { get; set; }
        public static JsonSerializerOptions Option { get; set; } = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = new CyberJsonTypeInfoResolver()
        };
    }
    public enum RunTimeEunm
    {
        DebugWindows,
        DebugLinux,
        ReleaseWindows,
        ReleaseLinux
    }

    /// <summary>
    /// CORS 跨域模块配置
    /// </summary>
    public class CorsModule
    {
        public string? Name { get; set; }
        public string[]? Origin { get; set; }
        public string[]? Headers { get; set; }
        public string[]? Methods { get; set; }
        public bool AllowCredentials { get; set; }
        public string[]? WithExposedHeaders { get; set; }
    }

    /// <summary>
    /// JWT 鉴权配置选项
    /// </summary>
    public class AuthOptions
    {
        /// <summary>
        /// 配置名称，用于支持多个JWT配置
        /// 如果不填，则不使用IOC的Keyed注册
        /// </summary>
        public string? Config { get; set; }

        /// <summary>
        /// Jwt认证Key
        /// </summary>
        public string Security { get; set; } = "";

        /// <summary>
        /// 过期时间【秒】
        /// </summary>
        public double Expiration { get; set; } = 1200;

        /// <summary>
        /// Header的Name
        /// </summary>
        public string HeaderName { get; set; }

        /// <summary>
        /// token验证错误返回信息
        /// </summary>
        public string ErrorMessage { get; set; } = "Unauthorized";

        /// <summary>
        /// JWT验证事件
        /// </summary>
        public Action<Microsoft.AspNetCore.Http.HttpContext, string>? Action { get; set; }

        /// <summary>
        /// 验证错误返回类型
        /// </summary>
        public AuthOptionEnum AuthOptionType { get; set; } = AuthOptionEnum.Json;
    }

    /// <summary>
    /// JWT验证错误返回类型
    /// </summary>
    public enum AuthOptionEnum
    {
        /// <summary>
        /// 返回JSON格式错误
        /// </summary>
        Json,

        /// <summary>
        /// 自定义Action处理
        /// </summary>
        Action
    }
}