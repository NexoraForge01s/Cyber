using System;

namespace Cyber.Http.Attributes
{
    /// <summary>
    /// HTTP 方法特性基类
    /// </summary>
    public abstract class HttpMethodAttribute : Attribute
    {
        /// <summary>
        /// 路径模板
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// HTTP 方法名称
        /// </summary>
        public abstract string Method { get; }

        protected HttpMethodAttribute(string path)
        {
            Path = path ?? "";
        }
    }
}