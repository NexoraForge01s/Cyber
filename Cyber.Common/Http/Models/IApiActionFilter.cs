using System.Net.Http;
using System.Threading.Tasks;

namespace Cyber.Http.Models
{
    /// <summary>
    /// 全局 API 动作过滤器接口
    /// </summary>
    public interface IApiActionFilter
    {
        /// <summary>
        /// 请求开始前执行
        /// </summary>
        Task OnBeginRequestAsync(ApiActionContext context);

        /// <summary>
        /// 请求结束后执行
        /// </summary>
        Task OnEndRequestAsync(ApiActionContext context);
    }
}