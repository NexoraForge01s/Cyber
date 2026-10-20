using System;
using System.Text;

namespace Cyber.Sinks.File
{
    /// <summary>
    /// File Logger 配置选项
    /// </summary>
    public class FileLoggerOptions
    {
        /// <summary>
        /// 日志文件路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 输出模板
        /// </summary>
        public string OutputTemplate { get; set; }

        /// <summary>
        /// 滚动间隔
        /// </summary>
        public RollingInterval RollingInterval { get; set; }

        /// <summary>
        /// 字符编码
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// 是否允许多进程共享日志文件
        /// </summary>
        public bool Shared { get; set; }

        /// <summary>
        /// 文件大小限制(字节)
        /// </summary>
        public long? FileSizeLimitBytes { get; set; }

        /// <summary>
        /// 是否在达到文件大小限制时滚动
        /// </summary>
        public bool RollOnFileSizeLimit { get; set; }

        /// <summary>
        /// 保留的文件数量限制
        /// </summary>
        public int? RetainedFileCountLimit { get; set; }

        /// <summary>
        /// 是否缓冲
        /// </summary>
        public bool Buffered { get; set; }

        /// <summary>
        /// 刷新到磁盘的间隔
        /// </summary>
        public TimeSpan? FlushToDiskInterval { get; set; }

        public LogSwitch LogSwitch { get; set; }


    }
}
