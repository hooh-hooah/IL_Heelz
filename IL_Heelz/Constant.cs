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

    public const string VERSION = "1.13.2";
    public static string[] parts = {"foot01", "foot02", "toes01"};
    public static string[] modifiers = {"move", "roll", "scale"};
    public const int ShoeCategory = 7;
    public const string kosiString = "BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Kosi01/cf_J_Kosi02/";

    public static Dictionary<string, string> pathMaps = new Dictionary<string, string>
    {
        {kosiString + "cf_J_LegUp00_L/cf_J_LegLow01_L/cf_J_LegLowRoll_L/cf_J_Foot01_L", "foot01"},
        {kosiString + "cf_J_LegUp00_R/cf_J_LegLow01_R/cf_J_LegLowRoll_R/cf_J_Foot01_R", "foot01"},
        {kosiString + "cf_J_LegUp00_L/cf_J_LegLow01_L/cf_J_LegLowRoll_L/cf_J_Foot01_L/cf_J_Foot02_L", "foot02"},
        {kosiString + "cf_J_LegUp00_R/cf_J_LegLow01_R/cf_J_LegLowRoll_R/cf_J_Foot01_R/cf_J_Foot02_R", "foot02"},
        {kosiString + "cf_J_LegUp00_L/cf_J_LegLow01_L/cf_J_LegLowRoll_L/cf_J_Foot01_L/cf_J_Foot02_L/cf_J_Toes01_L", "toes01"},
        {kosiString + "cf_J_LegUp00_R/cf_J_LegLow01_R/cf_J_LegLowRoll_R/cf_J_Foot01_R/cf_J_Foot02_R/cf_J_Toes01_R", "toes01"}
    };
}