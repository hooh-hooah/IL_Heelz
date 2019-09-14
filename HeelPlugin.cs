using System;
using System.Xml.Linq;
using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using AIChara;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI;
using BepInEx.Harmony;
using RootMotion.FinalIK;
using Sideloader.AutoResolver;
using System.Text;
using System.Xml;
using System.IO;

namespace Heelz
{
    public class HeelConfig
    {
        public Vector3 rootMove;
        public Dictionary<string, Dictionary<string, Vector3>> heelVectors = new Dictionary<string, Dictionary<string, Vector3>>();
        public Dictionary<string, bool> isFixed = new Dictionary<string, bool>();
        public bool loaded = false;
    }

    [BepInPlugin(GUID, "Heelz", VERSION)]
    [BepInDependency(Sideloader.Sideloader.GUID)]
    public partial class HeelPlugin : BaseUnityPlugin
    {
        public const string GUID = "com.hooh.heelz";
        public const string VERSION = "1.0.0";
        public static Dictionary<int, HeelConfig> heelConfigs = new Dictionary<int, HeelConfig>();
        public static String[] parts = { "foot01", "foot02", "toes01" };
        public static String[] modifiers = { "move", "roll", "scale" };
        public static String pathRoot = "BodyTop/p_cf_anim";
        readonly static string kosiString = "BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Kosi01/cf_J_Kosi02/";
        readonly static int shoeCategory = 7;
        public static Dictionary<string, string> pathMaps = new Dictionary<string, string>()
        {
            {kosiString + "cf_J_LegUp00_L/cf_J_LegLow01_L/cf_J_LegLowRoll_L/cf_J_Foot01_L", "foot01"},
            {kosiString + "cf_J_LegUp00_R/cf_J_LegLow01_R/cf_J_LegLowRoll_R/cf_J_Foot01_R", "foot01"},
            {kosiString + "cf_J_LegUp00_L/cf_J_LegLow01_L/cf_J_LegLowRoll_L/cf_J_Foot01_L/cf_J_Foot02_L", "foot02"},
            {kosiString + "cf_J_LegUp00_R/cf_J_LegLow01_R/cf_J_LegLowRoll_R/cf_J_Foot01_R/cf_J_Foot02_R", "foot02"},
            {kosiString + "cf_J_LegUp00_L/cf_J_LegLow01_L/cf_J_LegLowRoll_L/cf_J_Foot01_L/cf_J_Foot02_L/cf_J_Toes01_L", "toes01"},
            {kosiString + "cf_J_LegUp00_R/cf_J_LegLow01_R/cf_J_LegLowRoll_R/cf_J_Foot01_R/cf_J_Foot02_R/cf_J_Toes01_R", "toes01"},
        };
        public static ConfigWrapper<bool> isVerbose { get; private set; }
        public static ConfigWrapper<bool> loadDevXML { get; private set; }

        internal static new ManualLogSource Logger;

        void Log(string str) { if (isVerbose.Value) Logger.LogInfo(str); }

        private void Start()
        {
            Logger = base.Logger;
            isVerbose = Config.Wrap<bool>("Heelz", "Heelz Verbose Mode", "Heelz Verbose Mode", false);
            loadDevXML = Config.Wrap<bool>("Heelz", "Load Developer XML", "Load Developer XML", false);
            CharacterApi.RegisterExtraBehaviour<HeelsController>(GUID);
            HarmonyWrapper.PatchAll(typeof(HeelPlugin));

            Logger.LogInfo("[heelz] Heels mode activated: destroy all foot");
            var loadedManifests = Sideloader.Sideloader.LoadedManifests;
            foreach (var manifest in loadedManifests)
            {
                LoadXML(manifest.manifestDocument);
            }

            if (loadDevXML.Value)
            {
                try
                {
                    string rootPath = Directory.GetParent(Application.dataPath).ToString();
                    string devXMLPath = rootPath  + "/heel_manifest.xml";

                    if (File.Exists(devXMLPath))
                    {
                        Logger.LogError("Development File Detected! Updating heels as soon as it's getting updated!");

                        FileSystemWatcher fsWatcher = new FileSystemWatcher(rootPath.ToString()) { EnableRaisingEvents = true };
                        FileSystemEventHandler eventHandler = null;
                        eventHandler = (s, e) =>
                        {
                            XmlDocument devdoc = new XmlDocument();
                            devdoc.Load(devXMLPath);
                            Logger.LogWarning("Heel Development file has been updated!");
                            LoadXML(XDocument.Parse(devdoc.OuterXml), true);
                        };
                        fsWatcher.Changed += eventHandler;

                        fsWatcher.EnableRaisingEvents = true;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError("Tried to load heel Development XML, but failed!");
                    Logger.LogError(e.ToString());
                }
            }
        }

        private void LoadXML(XDocument manifestDocument, bool isDevelopment)
        {
            var heelDatas = manifestDocument?.Root?.Element("AI_HeelsData")?.Elements("heel");
            var guid = manifestDocument?.Root?.Element("guid").Value;
            if (heelDatas != null)
            {
                foreach (var element in heelDatas)
                {
                    int heelID = int.Parse(element.Attribute("id")?.Value);
                    ResolveInfo resolvedID = UniversalAutoResolver.TryGetResolutionInfo(heelID, "ChaFileClothes.ClothesShoes", guid);
                    if (resolvedID != null)
                    {
                        Logger.LogInfo(String.Format("Found Resolved ID: \"{0}\"=>\"{1}\"", heelID, resolvedID.LocalSlot));
                        heelID = resolvedID.LocalSlot;
                    }

                    heelConfigs.Remove(heelID);
                }
            }

            LoadXML(manifestDocument);
        }

        private void LoadXML(XDocument manifestDocument)
        {
            // Load XML and put all Heel Data on plugin's data dictionary.
            var heelDatas = manifestDocument?.Root?.Element("AI_HeelsData")?.Elements("heel");
            var guid = manifestDocument?.Root?.Element("guid").Value;

            if (heelDatas != null)
            {
                foreach (var element in heelDatas)
                {
                    int heelID = int.Parse(element.Attribute("id")?.Value);
                    if (heelConfigs.ContainsKey(heelID))
                    {
                        Logger.LogError(String.Format("CONFLITING HEEL DATA! Shoe ID {0} already has heel data.", heelID));
                        return;
                    }

                    Log(String.Format("Registering Heel Config for clothe ID: {0}", heelID));
                    if (heelID > -1)
                    {
                        Heelz.HeelConfig newConfig = new Heelz.HeelConfig();

                        try
                        {
                            foreach (String partKey in parts)
                            {
                                var partElement = element.Element(partKey);
                                if (partElement == null) continue;

                                // register position values such as roll, move scale.
                                // it will parse vec, min, max. but unfortunately, only "roll" will get the limitation feature.
                                // since we can't just make limit of vector... it's not going to move in most of case
                                Dictionary<string, Vector3> vectors = new Dictionary<string, Vector3>();
                                foreach (String modKey in modifiers)
                                {
                                    String[] split = partElement.Element(modKey)?.Attribute("vec")?.Value?.Split(',');
                                    if (split != null)
                                    {
                                        Vector3 vector = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
                                        vectors.Add(modKey, vector);
                                    } else vectors.Add(modKey, modKey=="scale"?Vector3.one:Vector3.zero); // Yeah.. if there is no scale, don't fuck it up.
                                    Log(String.Format("\t{0}_{1}: {2}", partKey, modKey, vectors[modKey].ToString()));

                                    if (modKey == "roll")
                                    {
                                        String[] mins = partElement.Element(modKey)?.Attribute("min")?.Value?.Split(',');
                                        String[] maxs = partElement.Element(modKey)?.Attribute("max")?.Value?.Split(',');
                                        if (mins != null)
                                        {
                                            vectors.Add(modKey + "min",new Vector3(float.Parse(mins[0]), float.Parse(mins[1]), float.Parse(mins[2])));
                                            Log(String.Format("\t{0}_{1}: {2}", partKey, modKey + "min", vectors[modKey + "min"].ToString()));
                                        }
                                        if (maxs != null)
                                        {
                                            vectors.Add(modKey + "max", new Vector3(float.Parse(maxs[0]), float.Parse(maxs[1]), float.Parse(maxs[2])));
                                            Log(String.Format("\t{0}_{1}: {2}", partKey, modKey + "max", vectors[modKey + "max"].ToString()));
                                        }
                                    }
                                }
                                newConfig.heelVectors.Add(partKey, vectors);

                                // register parent angle derivation.
                                String isFixed = partElement?.Attribute("fixed")?.Value;
                                if (isFixed != null)
                                {
                                    newConfig.isFixed.Add(partKey, bool.Parse(isFixed));
                                    Log(String.Format("\t{0}_isFixed: {1}", partKey, isFixed.ToString()));
                                }
                            }
                            String[] rootSplit = element.Element("root")?.Attribute("vec")?.Value?.Split(',');
                            if (rootSplit != null)
                                newConfig.rootMove = new Vector3(float.Parse(rootSplit[0]), float.Parse(rootSplit[1]), float.Parse(rootSplit[2]));
                            else newConfig.rootMove = Vector3.zero;

                            newConfig.loaded = true;

                            ResolveInfo resolvedID = UniversalAutoResolver.TryGetResolutionInfo(heelID, "ChaFileClothes.ClothesShoes", guid);
                            if (resolvedID != null)
                            {
                                Logger.LogInfo(String.Format("Found Resolved ID: \"{0}\"=>\"{1}\"", heelID, resolvedID.LocalSlot));
                                heelID = resolvedID.LocalSlot;
                            }

                            heelConfigs.Add(heelID, newConfig);
                            Logger.LogInfo(String.Format("Registered new heel config for follwing ID: \"{0}\"", heelID));
                        }
                        catch (Exception e) { Logger.LogError(e.ToString()); }
                    }
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCustomClothes))]
        public static void ChangeCustomClothes(ChaControl __instance, int kind)
        {
            if (kind == 7) 
                GetAPIController(__instance)?.SetUpShoes();
        }

        private static HeelsController GetAPIController(ChaControl character) => character?.gameObject?.GetComponent<HeelsController>();

        public class HeelsController : CharaCustomFunctionController
        {

            //not saving anything but still must implement
            protected override void OnCardBeingSaved(GameMode currentGameMode) { }
            protected override void OnReload(GameMode currentGameMode, bool maintainState) => SetUpShoes();

            public HeelConfig currentConfig;
            private Dictionary<Transform, Vector3[]> transformVectors = new Dictionary<Transform, Vector3[]>();
            private Dictionary<Transform, bool> parentDerivation = new Dictionary<Transform, bool>();

            public void SetUpShoes()
            {
                if (ChaControl.objClothes[shoeCategory] == null) return;

                int shoeID = ChaControl.nowCoordinate.clothes.parts[7].id;
                GameObject shoeGameObject = ChaControl.objClothes[7];
                HeelConfig shoeConfig;

                Logger.LogInfo(String.Format("Looking for ID: \"{0}\"", shoeID));
                if (heelConfigs.TryGetValue(shoeID, out shoeConfig) && shoeConfig.loaded == true)
                {
                    Logger.LogInfo("Found! Removing Transforms..");
                    RemoveLocalTransforms();
                    Logger.LogInfo("Found! Setting up Transforms..");
                    LocalTransforms(shoeConfig);
                }
                else
                {
                    Logger.LogInfo("Not Found! Removing Transforms..");
                    RemoveLocalTransforms();
                }
            }

            // You can't fly while sitting around or having a sex you moron
            public void DisableHover() => ChaControl.gameObject.transform.Find(pathRoot).localPosition = Vector3.zero;

            // start flying again cuz you're not getting laid
            public void EnableHover() => ChaControl.gameObject.transform.Find(pathRoot).localPosition = currentConfig.rootMove;
            
            public void RemoveLocalTransforms()
            {
                currentConfig = null;
                foreach (KeyValuePair<Transform, Vector3[]> macros in transformVectors)
                {
                    Transform targetTransform = macros.Key;

                    targetTransform.localPosition = Vector3.zero;
                    targetTransform.localScale = Vector3.one;
                    targetTransform.localEulerAngles = Vector3.zero;
                }
                ChaControl.gameObject.transform.Find(pathRoot).localPosition = Vector3.zero;

                transformVectors.Clear();
                parentDerivation.Clear();

                IKSolverFullBodyBiped ik = ChaControl.fullBodyIK.solver;
                ik.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(ik.OnPostUpdate, new IKSolver.UpdateDelegate(this.IKArray));
            }

            public void LocalTransforms(HeelConfig heelConfig)
            {
                currentConfig = heelConfig;
                // TODO: keep old value or calculate ABM or character value.

                ChaControl.gameObject.transform.Find(pathRoot).localPosition = heelConfig.rootMove;

                foreach(KeyValuePair<string, string> pair in pathMaps)
                {
                    Dictionary<string, Vector3> partVectors;
                    heelConfig.heelVectors.TryGetValue(pair.Value, out partVectors);

                    if (partVectors != null)
                    {
                        Vector3 roll, move, scale, rollMin, rollMax;
                        Transform thatShit = ChaControl.gameObject?.transform?.Find(pair.Key);

                        if (thatShit != null && 
                            partVectors.TryGetValue("roll", out roll) &&
                            partVectors.TryGetValue("move", out move) &&
                            partVectors.TryGetValue("scale", out scale))
                        {
                            partVectors.TryGetValue("rollmin", out rollMin);
                            partVectors.TryGetValue("rollmax", out rollMax);

                            transformVectors.Add(thatShit, new Vector3[5] {move, roll, scale, rollMin, rollMax});
                        }

                        bool parentDerive;
                        if (heelConfig.isFixed.TryGetValue(pair.Value, out parentDerive))
                            parentDerivation.Add(thatShit, parentDerive);
                    }
                }

                IKSolverFullBodyBiped ik = ChaControl.fullBodyIK.solver;
                ik.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(ik.OnPostUpdate, new IKSolver.UpdateDelegate(this.IKArray));
            }
            
            public void IKArray()
            {
                foreach (KeyValuePair<Transform, Vector3[]> macros in transformVectors)
                {
                    Transform targetTransform = macros.Key;

                    targetTransform.localPosition += macros.Value[0];
                    targetTransform.localScale = macros.Value[2];

                    if (parentDerivation.ContainsKey(targetTransform) && parentDerivation[targetTransform] == true)
                        targetTransform.eulerAngles = targetTransform.parent.eulerAngles;

                    targetTransform.localEulerAngles += macros.Value[1];

                    if (macros.Value[3] != Vector3.zero || macros.Value[4] != Vector3.zero)
                    {
                        if (macros.Value[3] != Vector3.zero)
                            targetTransform.localEulerAngles = new Vector3(
                                Math.Max(macros.Value[3].x, targetTransform.localEulerAngles.x),
                                Math.Max(macros.Value[3].y, targetTransform.localEulerAngles.y),
                                Math.Max(macros.Value[3].z, targetTransform.localEulerAngles.z)
                            );
                        if (macros.Value[4] != Vector3.zero)
                            targetTransform.localEulerAngles = new Vector3(
                                Math.Min(macros.Value[4].x, targetTransform.localEulerAngles.x),
                                Math.Min(macros.Value[4].y, targetTransform.localEulerAngles.y),
                                Math.Min(macros.Value[4].z, targetTransform.localEulerAngles.z)
                            );
                    }
                }
            }
        }
    }
}