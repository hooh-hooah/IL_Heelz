using AIChara;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Harmony;
using HarmonyLib;
using Heels.Controller;
using KKAPI.Chara;
using Util;

namespace Heelz
{
    [BepInPlugin(Constant.GUID, Constant.NAME, Constant.VERSION)]
    [BepInDependency(Sideloader.Sideloader.GUID)]
    public class HeelzPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> LoadDevXML { get; set; }
        public static ConfigEntry<bool> VerboseMode { get; set; }

        private void Start()
        {
            Util.Log.Logger.logSource = Logger;
            ConfigUtility.Initialize(Config);
            CharacterApi.RegisterExtraBehaviour<HeelsController>(Constant.GUID);
            HarmonyWrapper.PatchAll(typeof(HeelzPlugin));
            Logger.LogInfo("[Heelz] Heels mode activated: destroy all foot");
            var loadedManifests = Sideloader.Sideloader.Manifests.Values;
            foreach (var manifest in loadedManifests) XMLLoader.LoadXML(manifest.manifestDocument);
            if (LoadDevXML.Value) XMLLoader.StartWatchDevXML();
        }

        /*
         *  CLOTHES RELATED INTERACTIONS 
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCustomClothes))]
        public static void ChangeCustomClothes(ChaControl __instance, int kind)
        {
            if (kind == 7) GetAPIController(__instance)?.ApplyHeelsData();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetClothesState))]
        public static void SetClothesState(ChaControl __instance, int clothesKind, byte state, bool next = true)
        {
            if (clothesKind == 7) GetAPIController(__instance)?.UpdateHover();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), "LateUpdateForce")]
        public static void LateUpdateForce(ChaControl __instance)
        {
            var heelsController = GetAPIController(__instance);
            if (heelsController == null) return;
            if (!__instance.fullBodyIK.isActiveAndEnabled) heelsController.Handler.UpdateFootAngle();
        }

        private static HeelsController GetAPIController(ChaControl character)
        {
            return character?.gameObject?.GetComponent<HeelsController>();
        }
    }
}