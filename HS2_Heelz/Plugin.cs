using AIChara;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Heels.Controller;
using Util;
using KKAPI.Chara;
using UnityEngine;

namespace Heelz
{
    [BepInPlugin(Constant.GUID, Constant.NAME, Constant.VERSION)]
    [BepInDependency(Sideloader.Sideloader.GUID)]
    public class HeelzPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> LoadDevXML { get; set; }
        public static ConfigEntry<bool> VerboseMode { get; set; }

        internal static Harmony harmony;
        internal static ConfigEntry<bool> simpleHeelsInH;
        internal static ConfigEntry<bool> simpleHeelsInWorld;
        internal static ConfigEntry<bool> alwaysRotateHeels;

        private void Start()
        {
            Util.Log.Logger.logSource = Logger;
            ConfigUtility.Initialize(Config);
            CharacterApi.RegisterExtraBehaviour<HeelsController>(Constant.GUID);

            simpleHeelsInH = Config.Bind("Settings", "Simple Heels in HScenes", false, "Always adjust/rotate heels in HScenes, even when the character isn't standing.");
            simpleHeelsInWorld = Config.Bind("Settings", "Simple Heels on World Map", false, "Always adjust/rotate heels on the World Map, even when the character isn't standing.");
            alwaysRotateHeels = Config.Bind("Settings", "Always Rotate Heels", false, "Always rotate heels, even when the character isn't standing.");

            harmony = new Harmony("AI_Heelz");
            harmony.PatchAll(typeof(HeelzPlugin));
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
            if (kind == 7)
                GetAPIController(__instance)?.ApplyHeelsData();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetClothesState))]
        public static void SetClothesState(ChaControl __instance, int clothesKind, byte state, bool next = true)
        {
            if (clothesKind == 7)
                GetAPIController(__instance)?.UpdateHover();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), "LateUpdateForce")]
        public static void LateUpdateForce(ChaControl __instance)
        {
            var heelsController = GetAPIController(__instance);

            if (heelsController == null)
                return;

            if (!__instance.fullBodyIK.isActiveAndEnabled)
                heelsController.Handler.UpdateFootAngle();
        }

        //-- IK Solver Patch --//
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RootMotion.SolverManager), "LateUpdate")]
        public static void SolverManager_PreLateUpdate(RootMotion.SolverManager __instance)
        {
            ChaControl character = __instance.GetComponentInParent<ChaControl>();

            if (character == null)
                return;

            var heelsController = GetAPIController(character);

            if (heelsController == null)
                return;

            heelsController.Handler.UpdateEffectors();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Animator), "Play", typeof(string), typeof(int), typeof(float))]
        public static void Animator_Play(Animator __instance, string stateName)
        {
            var chaControl = __instance.GetComponentInParent<ChaControl>();

            if (chaControl == null)
                return;

            UpdateAnimation(chaControl, stateName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Animator), "PlayInFixedTime", typeof(string), typeof(int), typeof(float))]
        public static void Animator_PlayInFixedTime(Animator __instance, string stateName)
        {
            var chaControl = __instance.GetComponentInParent<ChaControl>();

            if (chaControl == null)
                return;

            UpdateAnimation(chaControl, stateName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Animator), "CrossFadeInFixedTime", typeof(string), typeof(float), typeof(int), typeof(float), typeof(float))]
        public static void Animator_CrossFadeInFixedTime(Animator __instance, string stateName)
        {
            var chaControl = __instance.GetComponentInParent<ChaControl>();

            if (chaControl == null)
                return;

            UpdateAnimation(chaControl, stateName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Animator), "CrossFade", typeof(string), typeof(float), typeof(int), typeof(float), typeof(float))]

        public static void Animator_CrossFade(Animator __instance, string stateName)
        {
            var chaControl = __instance.GetComponentInParent<ChaControl>();

            if (chaControl == null)
                return;

            UpdateAnimation(chaControl, stateName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Animator), "runtimeAnimatorController", MethodType.Setter)]
        public static void ActorAnimation_runtimeAnimatorController(Animator __instance)
        {
            if (__instance.runtimeAnimatorController == null || __instance.runtimeAnimatorController.name.IsNullOrEmpty())
                return;

            var chaControl = __instance.GetComponentInParent<ChaControl>();

            if (chaControl == null)
                return;

            UpdateAnimation(chaControl, "Idle");
        }

        private static void UpdateAnimation(ChaControl chaControl, string stateName)
        {
            var heelsController = GetAPIController(chaControl);

            if (heelsController == null)
                return;

            heelsController.UpdateAnimation(stateName);
        }

        private static HeelsController GetAPIController(ChaControl character)
        {
            return character?.gameObject?.GetComponent<HeelsController>();
        }
    }
}