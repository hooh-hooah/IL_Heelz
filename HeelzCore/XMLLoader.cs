using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Sideloader.AutoResolver;
using UnityEngine;

namespace HeelzCore
{
    public class XMLLoader
    {
        internal static void StartWatchDevXML()
        {
            try
            {
                var rootPath = Directory.GetParent(Application.dataPath).ToString();
                var devXMLPath = rootPath + "/heel_manifest.xml";

                if (File.Exists(devXMLPath))
                {
                    Util.Logger.Log("Development File Detected! Updating heels as soon as it's getting updated!");

                    var fsWatcher = new FileSystemWatcher(rootPath) {EnableRaisingEvents = true};
                    FileSystemEventHandler eventHandler = null;
                    eventHandler = (s, e) =>
                    {
                        var devdoc = new XmlDocument();
                        devdoc.Load(devXMLPath);
                        Util.Logger.Log("Heel Development file has been updated!");
                        LoadXML(XDocument.Parse(devdoc.OuterXml), true);
                    };
                    fsWatcher.Changed += eventHandler;

                    fsWatcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception e)
            {
                Util.Logger.Log("Tried to load heel Development XML, but failed!");
                Util.Logger.Log(e.ToString());
            }
        }

        internal static void LoadXML(XDocument manifestDocument, bool isDevelopment)
        {
            {
                var heelDatas = manifestDocument?.Root?.Element("AI_HeelsData")?.Elements("heel");
                var guid = manifestDocument?.Root?.Element("guid").Value;
                if (heelDatas != null)
                    foreach (var element in heelDatas)
                    {
                        var heelID = int.Parse(element.Attribute("id")?.Value);
                        var resolvedID =
                            UniversalAutoResolver.TryGetResolutionInfo(heelID, "ChaFileClothes.ClothesShoes", guid);
                        if (resolvedID != null)
                        {
                            Util.Logger.Log(string.Format("Found Resolved ID: \"{0}\"=>\"{1}\"", heelID,
                                resolvedID.LocalSlot));
                            heelID = resolvedID.LocalSlot;
                        }

                        Values.configs.Remove(heelID);
                    }

                LoadXML(manifestDocument);
            }
        }

        internal static void LoadXML(XDocument manifestDocument)
        {
            // Load XML and put all Heel Data on plugin's data dictionary.
            var heelDatas = manifestDocument?.Root?.Element("AI_HeelsData")?.Elements("heel");
            var guid = manifestDocument?.Root?.Element("guid").Value;
            if (heelDatas != null)
                foreach (var element in heelDatas)
                {
                    var heelID = int.Parse(element.Attribute("id")?.Value);
                    if (Values.configs.ContainsKey(heelID))
                    {
                        Util.Logger.Log(string.Format("CONFLITING HEEL DATA! Shoe ID {0} already has heel data.",
                            heelID));
                        return;
                    }

                    Util.Logger.Log(string.Format("Registering Heel Config for clothe ID: {0}", heelID));
                    if (heelID > -1)
                    {
                        var newConfig = new HeelConfig();

                        try
                        {
                            foreach (var partKey in Constant.parts)
                            {
                                var partElement = element.Element(partKey);
                                if (partElement == null)
                                    continue;

                                // register position values such as roll, move scale.
                                // it will parse vec, min, max. but unfortunately, only "roll" will get the limitation feature.
                                // since we can't just make limit of vector... it's not going to move in most of case
                                var vectors = new Dictionary<string, Vector3>();
                                foreach (var modKey in Constant.modifiers)
                                {
                                    var split = partElement.Element(modKey)?.Attribute("vec")?.Value?.Split(',');
                                    if (split != null)
                                    {
                                        var vector = new Vector3(float.Parse(split[0]), float.Parse(split[1]),
                                            float.Parse(split[2]));
                                        vectors.Add(modKey, vector);
                                    }
                                    else
                                    {
                                        vectors.Add(modKey,
                                            modKey == "scale"
                                                ? Vector3.one
                                                : Vector3.zero); // Yeah.. if there is no scale, don't fuck it up.
                                    }

                                    Util.Logger.Log(string.Format("\t{0}_{1}: {2}", partKey, modKey,
                                        vectors[modKey].ToString()));

                                    if (modKey == "roll")
                                    {
                                        var mins = partElement.Element(modKey)?.Attribute("min")?.Value
                                            ?.Split(',');
                                        var maxs = partElement.Element(modKey)?.Attribute("max")?.Value
                                            ?.Split(',');
                                        if (mins != null)
                                        {
                                            vectors.Add(modKey + "min",
                                                new Vector3(float.Parse(mins[0]), float.Parse(mins[1]),
                                                    float.Parse(mins[2])));
                                            Util.Logger.Log(string.Format("\t{0}_{1}: {2}", partKey, modKey + "min",
                                                vectors[modKey + "min"].ToString()));
                                        }

                                        if (maxs != null)
                                        {
                                            vectors.Add(modKey + "max",
                                                new Vector3(float.Parse(maxs[0]), float.Parse(maxs[1]),
                                                    float.Parse(maxs[2])));
                                            Util.Logger.Log(string.Format("\t{0}_{1}: {2}", partKey, modKey + "max",
                                                vectors[modKey + "max"].ToString()));
                                        }
                                    }
                                }

                                newConfig.heelVectors.Add(partKey, vectors);

                                // register parent angle derivation.
                                var isFixed = partElement?.Attribute("fixed")?.Value;
                                if (isFixed != null)
                                {
                                    newConfig.isFixed.Add(partKey, bool.Parse(isFixed));
                                    Util.Logger.Log(string.Format("\t{0}_isFixed: {1}", partKey, isFixed));
                                }
                            }

                            var rootSplit = element.Element("root")?.Attribute("vec")?.Value?.Split(',');
                            if (rootSplit != null)
                                newConfig.rootMove = new Vector3(float.Parse(rootSplit[0]), float.Parse(rootSplit[1]),
                                    float.Parse(rootSplit[2]));
                            else
                                newConfig.rootMove = Vector3.zero;

                            newConfig.loaded = true;

                            var resolvedID =
                                UniversalAutoResolver.TryGetResolutionInfo(heelID, "ChaFileClothes.ClothesShoes", guid);
                            if (resolvedID != null)
                            {
                                Util.Logger.Log(string.Format("Found Resolved ID: \"{0}\"=>\"{1}\"", heelID,
                                    resolvedID.LocalSlot));
                                heelID = resolvedID.LocalSlot;
                            }

                            Values.configs.Add(heelID, newConfig);
                            Util.Logger.Log(string.Format("Registered new heel ID: \"{0}\"", heelID));
                        }
                        catch (Exception e)
                        {
                            Util.Logger.Log(e.ToString());
                        }
                    }
                }
        }
    }
}