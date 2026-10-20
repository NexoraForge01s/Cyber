using System;
using System.Collections.Generic;
using System.Text;

namespace Cyber
{
    /// <summary>
    /// 生命周期枚举。映射到 Microsoft.Extensions.DependencyInjection 的 ServiceLifetime。
    /// </summary>
    public enum AutofacScope
    {
        /// <summary>每次请求都创建新实例（默认）。对应 Transient。</summary>
        InstancePerDependency = 0,

        /// <summary>单例。对应 Singleton。</summary>
        SingleInstance = 1,

        /// <summary>每个 Scope 一个实例。对应 Scoped。</summary>
        InstancePerLifetimeScope = 2,

    }

    /// <summary>
    /// 实例所有权枚举。
    /// </summary>
    public enum ComponentOwnership
    {
        /// <summary>由 DI 容器管理（默认）。容器释放时由容器调用 Dispose。</summary>
        LifetimeScope = 0,

        /// <summary>由使用者自行管理生命周期。容器不会自动调用 Dispose。</summary>
        External = 1,
    }

    /// <summary>
    /// 属性注入策略。
    /// </summary>
    public enum InjectPropertyType
    {
        /// <summary>仅注入打了 [Autowired] 标签的属性（默认）。</summary>
        Autowired = 0,

        /// <summary>注入全部可解析的公共属性（按属性名 + 类型回退）。</summary>
        All = 1,

    }
}
