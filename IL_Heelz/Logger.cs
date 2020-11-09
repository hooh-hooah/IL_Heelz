using BepInEx.Logging;

namespace Util
{
    public static class Logger
    {
        public static ManualLogSource logSource;

        internal static void Log(object message)
        {
            if (!ConfigUtility.CanLog) return;
            logSource.LogDebug(message);
        }
    }
}