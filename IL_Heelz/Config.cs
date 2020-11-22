using BepInEx.Configuration;
using Heelz;

namespace Util
{
    public static class ConfigUtility
    {
        public static bool CanLog => HeelzPlugin.VerboseMode.Value;

        public static void Initialize(ConfigFile Config)
        {
            HeelzPlugin.LoadDevXML = Config.Bind("Heelz", "Load Developer XML", false,
                new ConfigDescription("Make Heelz Plugin load heel_manifest.xml file from game root folder. Useful for developing heels. Useless for most of users."));
            HeelzPlugin.VerboseMode = Config.Bind("Heelz", "Verbose Mode", false,
                new ConfigDescription("Print Everything"));
        }
    }
}