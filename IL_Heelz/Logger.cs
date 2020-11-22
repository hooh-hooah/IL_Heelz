using System;
using BepInEx.Logging;

namespace Util.Log
{
    internal enum Category
    {
        Default,
        Handler,
        Controller,
        Utility
    }

    public static class Logger
    {
        public static ManualLogSource logSource;

        internal static void Log(object message)
        {
            Log(Category.Default, message);
        }

        internal static void Log(Category category, object message)
        {
            if (!ConfigUtility.CanLog || message == null) return;
            logSource?.LogDebug($"{Enum.GetName(typeof(Category), category)} {message}");
        }
    }
}