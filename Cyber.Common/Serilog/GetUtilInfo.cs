using Cyber.Sinks.File;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyber
{
    public static class GetUtilInfo
    {

        /// <summary>
        /// 配置日志写入管道,支持自定义 File 和 Async 配置
        /// </summary>
        private static FileLoggerOptions ConfigureLogPipeline(
            Cyber.Configuration.LoggerSinkConfiguration log,
            string logPath,
            LogSetting logSetting)
        {
            // 创建默认配置选项
            FileLoggerOptions fileLoggerOptions = new FileLoggerOptions();
            fileLoggerOptions.Shared = false;
            fileLoggerOptions.OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [<{ThreadId}>] {Message:lj}{NewLine}{Exception} \r";
            fileLoggerOptions.RollingInterval = RollingInterval.Day;
            fileLoggerOptions.Encoding = Encoding.UTF8;
            fileLoggerOptions.Path = logPath;

            // 如果配置了 CustomFileConfiguration,使用它来修改配置
            if (logSetting?.CustomFileConfiguration != null)
            {
                logSetting.CustomFileConfiguration(fileLoggerOptions);
            }

            return fileLoggerOptions;
        }

        /// <summary>
        /// 获取日志实例
        /// </summary>
        /// <returns></returns>
        public static ILog GetLogger()
        {
            ILog log = AppGlobalSettings.CyberSetting?.ILog;
            if (log == null)
            {
                var logSetting = AppGlobalSettings.CyberSetting?.GetOption<LogSetting>();
                var loggerConfig = new Serilog.LoggerConfiguration()
                                     .MinimumLevel.Debug()
                                     .Enrich.WithThreadId();

                if (logSetting?.CustomLoggerConfiguration != null)
                {
                    logSetting.CustomLoggerConfiguration(loggerConfig);
                }

                return loggerConfig
                     .WriteTo
                    .MapExt("Cyber", (me, log) =>
                    log.Map<Tuple<DateTime, Cyber.LogEventLevel>>(m => Tuple.Create(new DateTime(m.Timestamp.Year, m.Timestamp.Month, m.Timestamp.Day), m.Level),
                        (key, log) =>
                        {
                            var logPath = $"{AppContext.BaseDirectory}{System.IO.Path.DirectorySeparatorChar}Logs{System.IO.Path.DirectorySeparatorChar}{key.Item2}{System.IO.Path.DirectorySeparatorChar}{key.Item1.ToString("yyyy-MM-dd")}{System.IO.Path.DirectorySeparatorChar}{me}_.log";
                            var options = ConfigureLogPipeline(log, logPath, logSetting);
                            log.Async(w => w.File(options), 10000);
                        })).CreateLogger();
            }
            else
                return log;
        }

    }
}
