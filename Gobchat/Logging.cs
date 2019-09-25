using System;
using RainbowMage.OverlayPlugin;

namespace Gobchat
{
    public interface Logger
    {
        void LogDebug(String format, params object[] args);
        void LogError(String format, params object[] args);
        void LogWarning(String format, params object[] args);
        void LogInfo(String format, params object[] args);
    }

    public class MainLogger : Logger
    {
        public delegate void PostLog(LogLevel level, string format, params object[] args);

        private readonly PostLog logger;

        public MainLogger(PostLog logger)
        {
            this.logger = logger;
        }

        public void LogDebug(string format, params object[] args)
        {
            logger(LogLevel.Debug, format, args);
        }

        public void LogError(string format, params object[] args)
        {
            logger(LogLevel.Error, format, args);
        }

        public void LogInfo(string format, params object[] args)
        {
            logger(LogLevel.Info, format, args);
        }

        public void LogWarning(string format, params object[] args)
        {
            logger(LogLevel.Warning, format, args);
        }
    }
}
