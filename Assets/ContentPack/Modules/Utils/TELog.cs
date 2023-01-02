using BepInEx.Logging;
using System.Runtime.CompilerServices;

namespace TurboEdition
{
    public class TELog
    {
        public static ManualLogSource logger = null;
        public static bool outputAlways = false;

        public TELog(ManualLogSource logger_)
        {
            logger = logger_;
        }

        public static void LogD(object data, bool alwaysLog = false, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            if (logger != null && (outputAlways || alwaysLog))
                logger.LogDebug(logString(data, i, member));
        }

        public static void LogE(object data, bool alwaysLog = false, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            if (logger != null && (outputAlways || alwaysLog))
                logger.LogError(logString(data, i, member));
        }

        public static void LogF(object data, bool alwaysLog = false, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            if (logger != null && (outputAlways || alwaysLog))
                logger.LogFatal(logString(data, i, member));
        }

        public static void LogI(object data, bool alwaysLog = false, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            if (logger != null && (outputAlways || alwaysLog))
                logger.LogInfo(logString(data, i, member));
        }

        public static void LogM(object data, bool alwaysLog = false, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            if (logger != null && (outputAlways || alwaysLog))
                logger.LogMessage(logString(data, i, member));
        }

        public static void LogW(object data, bool alwaysLog = false, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            if (logger != null && (outputAlways || alwaysLog))
                logger.LogWarning(logString(data, i, member));
        }

        private static string logString(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            return string.Format("{0} :: Line: {1}, Method {2}", data, i, member);
        }
    }
}