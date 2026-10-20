using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cyber
{
    
    public class NeedLoginException : Exception
    {
        public List<string> _ArrayData { get; set; }
        /// <summary>
        /// 根据前端协议 用此方法返回的错误是401 前端会跳转到登录页
        /// 支持DI构造函数注入抛出异常
        /// </summary>
        public NeedLoginException(string? message) : base(message)
        {
        }
        /// <summary>
        /// 根据前端协议 用此方法返回的错误是401 前端会跳转到登录页
        /// 支持DI构造函数注入抛出异常
        /// </summary>
        public NeedLoginException(List<string> ArrayData)
        {
            _ArrayData = ArrayData;
        }
    }
}
