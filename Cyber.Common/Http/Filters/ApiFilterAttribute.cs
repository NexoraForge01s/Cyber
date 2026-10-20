using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cyber.Http.Models;
using Cyber.Http.Extensions;

namespace Cyber.Http.Filters
{
    /// <summary>
    /// API 过滤器特性基类
    /// </summary>
    public abstract class ApiFilterAttributeBase : Attribute, IApiActionFilter
    {
        /// <summary>
        /// 请求前执行
        /// </summary>
        public virtual Task OnBeginRequestAsync(ApiActionContext context)
        {
            var requestContext = new ApiRequestContext
            {
                RequestMessage = context.RequestMessage,
                ActionContext = context
            };
            return OnRequestAsync(requestContext);
        }

        /// <summary>
        /// 请求后执行
        /// </summary>
        public virtual Task OnEndRequestAsync(ApiActionContext context)
        {
            var responseContext = new ApiResponseContext
            {
                ResponseMessage = context.RequestMessage.GetResponseOrNull() ?? new HttpResponseMessage(),
                ActionContext = context
            };
            return OnResponseAsync(responseContext);
        }

        /// <summary>
        /// 请求阶段（自定义过滤器实现此方法）
        /// </summary>
        public virtual Task OnRequestAsync(ApiRequestContext context)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 响应阶段（自定义过滤器实现此方法）
        /// </summary>
        public virtual Task OnResponseAsync(ApiResponseContext context)
        {
            return Task.CompletedTask;
        }
    }
}