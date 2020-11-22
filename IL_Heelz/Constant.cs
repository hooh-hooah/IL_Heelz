using System.Collections.Generic;

public static class Constant
{
    // TODO: finish animation lists for Heelz
#if HS2
        public const string GUID = "com.hooh.hs2.heelz";
        public const string NAME = "HS2Heelz";
        public static List<int> StandingAnimations = new List<int>() {
            22,
            28,
            35,
            36,
            39,
            12,
            13,
            17,
            25
        };
#endif
    public const string GUID = "com.hooh.ai.heelz";
    public const string NAME = "Heelz";
    public static List<int> StandingAnimations = new List<int>();

    public const string VERSION = "1.14.3";
    public const int ShoeCategory = 7;
}