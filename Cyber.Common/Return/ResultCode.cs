using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyber
{
    /// <summary>
    /// 返回码
    /// </summary>
    public enum ResultCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 200,

        /// <summary>
        /// token失效 需重新登陆
        /// </summary>
        NeedLogin = 401,

        /// <summary>
        /// 传入参数错误
        /// </summary>
        ParameterError = 450,

        /// <summary>
        /// 程序异常
        /// </summary>
        Exception = 500,

        /// <summary>
        /// 找不到该方法
        /// </summary>
        NoFound = 404,


        NoMethod=405


    }
}
