﻿using System.Runtime.CompilerServices;
using AIChara;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Harmony;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Chara;

namespace Heelz
{
    [BepInPlugin(Constant.GUID, Constant.NAME, Constant.VERSION)]
    [BepInDependency(Sideloader.Sideloader.GUID)]
    public class HeelzPlugin : BaseUnityPlugin
    {
        internal new static ManualLogSource Logger;
        public static ConfigEntry<bool> LoadDevXML { get; private set; }

        private void Start()
        {
            Logger = base.Logger;
            Util.Logger.logSource = Logger;
            LoadDevXML = Config.Bind("Heelz", "Load Developer XML", false,
                new ConfigDescription("Make Heelz Plugin load heel_manifest.xml file from game root folder. Useful for developing heels. Useless for most of users."));
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
            if (kind == 7) GetAPIController(__instance)?.SetUpShoes();
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
            if (!__instance.fullBodyIK.isActiveAndEnabled) heelsController.IKArray();
        }

        private static HeelsController GetAPIController(ChaControl character)
        {
            return character?.gameObject?.GetComponent<HeelsController>();
        }
    }
}