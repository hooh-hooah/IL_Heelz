using BepInEx.Logging;
using UnityEngine;

namespace HeelzCore
{
    public class Util
    {
        public class Logger
        {
            public static ManualLogSource logger;

            internal static void Log(object message)
            {
#if TEST
                Console.WriteLine(message);
#else
                Debug.Log(message);
#endif
            }
        }
    }
}