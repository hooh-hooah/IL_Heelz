using System.IO;
using System.Xml;
using AIChara;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Harmony;
using BepInEx.Logging;
using HarmonyLib;
using HeelzCore;
using KKAPI.Chara;

namespace Heelz
{
    [BepInPlugin(Constant.GUID, Constant.NAME, Constant.VERSION)]
    [BepInDependency(Sideloader.Sideloader.GUID)]
    public class HSHeelz : BaseUnityPlugin
    {
        public const string GUID = "com.hooh.heelz";
        public const string VERSION = "1.13.0";

        internal new static ManualLogSource Logger;

        public static ConfigEntry<bool> IsVerbose { get; private set; }

        public static ConfigEntry<bool> LoadDevXML { get; private set; }

        private void Start()
        {
            Logger = base.Logger;
            Util.Logger.logger = Logger;

            IsVerbose = Config.Bind("Heelz", "Heelz Verbose Mode", false,
                new ConfigDescription("Make Heelz Plugin print all of debug messages in console. Useless for most of users."));
            LoadDevXML = Config.Bind("Heelz", "Load Developer XML", false,
                new ConfigDescription("Make Heelz Plugin load heel_manifest.xml file from game root folder. Useful for developing heels. Useless for most of users."));
            CharacterApi.RegisterExtraBehaviour<HeelsController>(GUID);
            HarmonyWrapper.PatchAll(typeof(HSHeelz));

            Logger.LogInfo("[Heelz] Heels mode activated: destroy all foot");
            var loadedManifests = Sideloader.Sideloader.Manifests.Values;
            foreach (var manifest in loadedManifests) XMLLoader.LoadXML(manifest.manifestDocument);

            if (LoadDevXML.Value) XMLLoader.StartWatchDevXML();
        }

        /*
         * ANIMATION BASED INTERACTIONS
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HScene), nameof(HScene.ChangeAnimation))]
        public static void OnChangeAnimation(HScene __instance, HScene.AnimationListInfo _info, bool _isForceResetCamera, bool _isForceLoopAction = false, bool _UseFade = true)
        {
            var isGroundAnimation = Constant.StandingAnimations.Contains(_info.id);
            var females = __instance.GetFemales();
            foreach (var female in females)
            {
                var femaleApiController = GetAPIController(female);
                if (femaleApiController != null)
                {
                    femaleApiController.GroundAnim = isGroundAnimation;
                    femaleApiController?.UpdateHover();
                }
            }
        }

        /*
         *  CLOTHES RELATED INTERACTIONS 
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCustomClothes))]
        public static void ChangeCustomClothes(ChaControl __instance, int kind)
        {
            if (kind == 7)
                GetAPIController(__instance)?.SetUpShoes();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetClothesState))]
        public static void SetClothesState(ChaControl __instance, int clothesKind, byte state, bool next = true)
        {
            if (clothesKind == Constant.shoeCategory)
                GetAPIController(__instance)?.UpdateHover();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), "LateUpdateForce")]
        public static void LateUpdateForce(ChaControl __instance)
        {
            var heelsController = GetAPIController(__instance);
            if (heelsController != null)
            {
                if (!__instance.fullBodyIK.isActiveAndEnabled)
                    heelsController.IKArray();
            }
        }

        private static HeelsController GetAPIController(ChaControl character)
        {
            return character?.gameObject?.GetComponent<HeelsController>();
        }
    }
}
