using System;
using System.Collections.Generic;
using System.Text;

namespace Cyber
{
    /// <summary>
    /// 标记属性/字段为需要由 DI 容器自动注入。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class AutowiredAttribute : Attribute
    {
        /// <summary>对应的服务注册 Key（用于按 Key 解析）。</summary>
        public string Key { get; set; }

        /// <summary>显式指定要解析的服务类型。默认为 null，表示使用属性/字段的类型。</summary>
        public Type? Service { get; set; }

        /// <summary>当容器中找不到对应服务时，是否允许返回 null（不抛异常）。默认 false。</summary>
        public bool Required { get; set; } = true;

        public AutowiredAttribute() { }

        public AutowiredAttribute(string key)
        {
            Key = key;
        }
    }
}

