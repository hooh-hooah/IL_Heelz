using System.Collections.Generic;

public static class Constant
{
#if HS2
    public const string GUID = "com.hooh.hs2.heelz";
    public const string NAME = "HS2Heelz";
#else
    public const string GUID = "com.hooh.ai.heelz";
    public const string NAME = "AIHeelz";
#endif
    public const string VERSION = "1.15.4";
    public const int ShoeCategory = 7;
}