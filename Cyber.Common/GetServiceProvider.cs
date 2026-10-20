using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace Cyber
{
    /// <summary>
    /// 全局 DI 容器服务获取工具类
    /// 支持：
    ///   1. 普通服务解析（GetService）
    ///   2. Keyed 服务解析（全局 + 作用域）
    ///   3. 作用域服务解析（仅限 Web API 场景）
    /// </summary>
    public static class GetServiceProvider
    {
        /// <summary>
        /// 全局 IServiceProvider 实例（由 Startup 或 Program 设置）
        /// </summary>
        public static IServiceProvider? Instance { get; set; }

        private static readonly AsyncLocal<HttpScopeServices?> _currentScope = new AsyncLocal<HttpScopeServices?>();

        public static void SetCurrentScope(HttpContext? HttpContext, IServiceProvider ServiceProvider)
        {
            _currentScope.Value = new HttpScopeServices(HttpContext,ServiceProvider);
        }

        public static void ClearCurrentScope()
        {
            _currentScope.Value = null;
        }

        public static HttpScopeServices? GetCurrentScope()
        {
            return _currentScope?.Value;
        }


        #region 非 Keyed 服务解析

        /// <summary>
        /// 从全局容器获取服务实例（非 Keyed）
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>服务实例，失败返回 default(T)</returns>
        public static T? GetService<T>()
        {
            if (Instance == null) return default;
            try
            {
                return Instance.GetService<T>();
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 从全局容器获取服务实例（非 Keyed），支持自定义错误返回值
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="errorReturn">解析失败时返回的默认值</param>
        /// <returns>服务实例或 errorReturn</returns>
        public static T? GetService<T>(T errorReturn)
        {
            if (Instance == null) return errorReturn;
            try
            {
                var service = Instance.GetService<T>();
                return service is T t ? t : errorReturn;
            }
            catch
            {
                return errorReturn;
            }
        }

        #endregion

        #region Keyed 服务解析（全局容器）

        /// <summary>
        /// 【全局容器】通过 Key 获取注册的 Keyed 服务
        /// 支持：services.AddKeyedSingleton&lt;T&gt;("key", impl)
        /// 注意：作用域服务（Scoped）在此处会表现为 Singleton 行为
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="key">注册时使用的 Key（string / enum / int 等）</param>
        /// <returns>服务实例，失败返回 default(T)</returns>
        public static T? GetKeyedService<T>(object key)
        {
            if (Instance == null) return default;

            try
            {
                return Instance.GetKeyedService<T>(key);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 【全局容器】通过 Key 获取 Keyed 服务，支持自定义错误返回值
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="key">注册时使用的 Key</param>
        /// <param name="errorReturn">解析失败时返回的默认值</param>
        /// <returns>服务实例或 errorReturn</returns>
        public static T? GetKeyedService<T>(object key, T errorReturn)
        {
            if (Instance == null) return errorReturn;

            try
            {
                var service = Instance.GetKeyedService<T>(key);
                return service ?? errorReturn;
            }
            catch
            {
                return errorReturn;
            }
        }


        #endregion

        #region 作用域服务解析（仅限 Web API 场景）

        /// <summary>
        /// 【作用域容器】获取当前 HTTP 请求作用域内的服务（非 Keyed）
        /// 必须在 Web API 请求上下文中调用
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>作用域服务实例，失败返回 default(T)</returns>
        public static T? GetScopeService<T>()
        {
           
            var scope = GetCurrentScope();
            if (scope != null)
            {
                try
                {
                    return scope.ServiceProvider.GetService<T>();
                }
                catch
                {
                    return default;
                }
            }

            return default;
        }

        /// <summary>
        /// 【作用域容器】获取作用域服务，支持自定义错误返回值
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="errorReturn">解析失败时返回的默认值</param>
        /// <returns>服务实例或 errorReturn</returns>
        public static T? GetScopeService<T>(T errorReturn)
        {
          
            var scope = GetCurrentScope();
            if (scope != null)
            {
                try
                {
                    var service = scope.ServiceProvider.GetService<T>();
                    return service is T t ? t : errorReturn;
                }
                catch
                {
                    return errorReturn;
                }
            }

            return errorReturn;
        }

        /// <summary>
        /// 【作用域容器】通过 Key 获取当前请求作用域内的 Keyed 服务
        /// 推荐用于 Scoped / Transient 的 Keyed 服务
        /// 确保：已注册 IHttpContextAccessor
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="key">注册时使用的 Key</param>
        /// <returns>作用域内的服务实例，失败返回 default(T)</returns>
        public static T? GetScopeKeyedService<T>(object key)
        {
            var scope = GetCurrentScope();
            if (scope != null)
            {
                try
                {
                    return scope.ServiceProvider.GetKeyedService<T>(key);
                }
                catch
                {
                    return default;
                }
            }
            return default;
        }

        /// <summary>
        /// 【作用域容器】通过 Key 获取 Keyed 服务，支持自定义错误返回值
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="key">注册时使用的 Key</param>
        /// <param name="errorReturn">解析失败时返回的默认值</param>
        /// <returns>服务实例或 errorReturn</returns>
        public static T? GetScopeKeyedService<T>(object key, T errorReturn)
        {
          
            var scope = GetCurrentScope();
            if (scope != null)
            {
                try
                {
                    var service = scope.ServiceProvider.GetKeyedService<T>(key);
                    return service ?? errorReturn;
                }
                catch
                {
                    return errorReturn;
                }
            }
            return errorReturn;
        }

        #endregion
    }

    public record HttpScopeServices(HttpContext? HttpContext, IServiceProvider ServiceProvider);

    

}