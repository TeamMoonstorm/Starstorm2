using BepInEx.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UDebug = UnityEngine.Debug;

namespace SS2
{
    internal class SS2Log
    {
        private static ManualLogSource logger = null;

#if DEBUG && !UNITY_EDITOR
        private static LogLevel _breakableLevel = LogLevel.Fatal;
#endif

        public static void Fatal(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "") => Log(LogLevel.Fatal, data, i, member);

        public static void Error(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "") => Log(LogLevel.Error, data, i, member);

        public static void Warning(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "") => Log(LogLevel.Warning, data, i, member);

        public static void Message(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "") => Log(LogLevel.Message, data, i, member);

        public static void Info(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "") => Log(LogLevel.Info, data, i, member);

        public static void Debug(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "") => Log(LogLevel.Debug, data, i, member);

        private static void Log(LogLevel level, object data, int i, string member)
        {
#if UNITY_EDITOR
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                case LogLevel.Message:
                    UDebug.Log(data);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    UDebug.LogError(Format(data, i, member));
                    break;
                case LogLevel.Warning:
                    UDebug.LogWarning(Format(data, i, member));
                    break;
            }
#else
            object data2 = (level.HasFlag(LogLevel.Warning) || level.HasFlag(LogLevel.Error) || level.HasFlag(LogLevel.Fatal)) ? Format(data, i, member) : data;

            _log.Log(level, data2);
#if DEBUG
            if(_breakableLevel.HasFlag(level))
            {
                TryBreak();
            }
#endif
#endif
        }

        public static string Format(object data, int i, string member)
        {
#if UNITY_EDITOR
            return $"[SS2Log]: {data} | Line={i} : Member={member}";
#else
            return $"{data} | Line={i} : Member={member}";
#endif
        }

#if DEBUG
        private static void TryBreak()
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }
#endif
        internal SS2Log(ManualLogSource logger_)
        {
            logger = logger_;
        }
    }
}
