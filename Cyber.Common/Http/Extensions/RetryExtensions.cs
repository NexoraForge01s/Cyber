using System;
using System.Threading.Tasks;

namespace Cyber.Http.Extensions
{
    /// <summary>
    /// Task 重试扩展方法
    /// </summary>
    public static class RetryExtensions
    {
        /// <summary>
        /// 重试执行异步任务
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="taskFactory">任务工厂</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="delayFactory">延迟工厂（参数为当前重试次数）</param>
        /// <returns>任务结果</returns>
        public static async Task<T> Retry<T>(this Func<Task<T>> taskFactory, int maxRetries, Func<int, TimeSpan> delayFactory)
        {
            for (int i = 0; i <= maxRetries; i++)
            {
                try
                {
                    return await taskFactory();
                }
                catch (Exception) when (i < maxRetries)
                {
                    var delay = delayFactory(i);
                    await Task.Delay(delay);
                }
            }

            // 最后一次尝试
            return await taskFactory();
        }

        /// <summary>
        /// 重试执行异步任务（无返回值）
        /// </summary>
        /// <param name="taskFactory">任务工厂</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="delayFactory">延迟工厂（参数为当前重试次数）</param>
        public static async Task Retry(this Func<Task> taskFactory, int maxRetries, Func<int, TimeSpan> delayFactory)
        {
            for (int i = 0; i <= maxRetries; i++)
            {
                try
                {
                    await taskFactory();
                    return;
                }
                catch (Exception) when (i < maxRetries)
                {
                    var delay = delayFactory(i);
                    await Task.Delay(delay);
                }
            }

            // 最后一次尝试
            await taskFactory();
        }
    }
}