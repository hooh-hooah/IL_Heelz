using BepInEx.Logging;

namespace Util
{
    public static class Logger
    {
        public static ManualLogSource logSource;

        internal static void Log(object message)
        {
            if (!ConfigUtility.CanLog || message == null) return;
            logSource?.LogDebug(message);
        }
    }
}