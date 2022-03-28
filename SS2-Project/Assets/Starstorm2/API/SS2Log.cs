using BepInEx.Logging;
using System.Runtime.CompilerServices;

namespace Moonstorm.Starstorm2
{
    public class SS2Log
    {
        public static ManualLogSource logger = null;

        public SS2Log(ManualLogSource logger_)
        {
            logger = logger_;
        }

        internal static void Debug(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            logger.LogDebug(logString(data, i, member));
        }
        internal static void Error(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            logger.LogError(logString(data, i, member));
        }
        internal static void Fatal(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            logger.LogFatal(logString(data, i, member));
        }
        internal static void Info(object data)
        {
            logger.LogInfo(data);
        }
        internal static void Message(object data)
        {
            logger.LogMessage(data);
        }
        internal static void Warning(object data)
        {
            logger.LogWarning(data);
        }

        private static string logString(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            return string.Format("{0} :: Line: {1}, Method {2}", data, i, member);
        }
    }
}
