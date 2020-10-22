using System.Collections.Generic;
using UnityEngine;

public class HeelConfig
{
    public readonly Dictionary<string, Dictionary<string, Vector3>> heelVectors = new Dictionary<string, Dictionary<string, Vector3>>();
    public readonly Dictionary<string, bool> isFixed = new Dictionary<string, bool>();
    public bool loaded = false;
    public Vector3 rootMove;
}

public class Values
{
    public static readonly Dictionary<int, HeelConfig> Configs = new Dictionary<int, HeelConfig>();
}