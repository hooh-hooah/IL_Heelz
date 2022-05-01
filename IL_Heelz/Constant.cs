using System.Collections.Generic;

public static class Constant
{
    // TODO: finish animation lists for Heelz
#if HS2
    public const string GUID = "com.hooh.hs2.heelz";
    public const string NAME = "HS2Heelz";
#else
    public const string GUID = "com.hooh.ai.heelz";
    public const string NAME = "AIHeelz";
#endif
    public const string VERSION = "2.0.0";
    public const int ShoeCategory = 7;
}