using System;

namespace Cyber
{
    /// <summary>
    /// 标记 MiniAPI 端点忽略重复请求拦截
    /// 可标记在类（控制器）或方法上
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class IgnoreRepeatedRequestAttribute : Attribute
    {
    }
}
