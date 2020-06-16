using System.Collections.Generic;
using UnityEngine;

namespace HeelzCore
{
    public class HeelConfig
    {
        public Dictionary<string, Dictionary<string, Vector3>> heelVectors = new Dictionary<string, Dictionary<string, Vector3>>();
        public Dictionary<string, bool> isFixed = new Dictionary<string, bool>();
        public bool loaded = false;
        public Vector3 rootMove;
    }

    public class Values
    {
        public static Dictionary<int, HeelConfig> configs = new Dictionary<int, HeelConfig>();
        public static object HeelConfig { get; set; }
    }
}