using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Sideloader.AutoResolver;
using UnityEngine;
using Logger = Util.Logger;

public static class XMLLoader
{
    internal static void StartWatchDevXML()
    {
        try
        {
            var rootPath = Directory.GetParent(Application.dataPath).ToString();
            var devXMLPath = rootPath + "/heel_manifest.xml";

            if (!File.Exists(devXMLPath)) return;
            Logger.Log("Development File Detected! Updating heels as soon as it's getting updated!");

            var fsWatcher = new FileSystemWatcher(rootPath) {EnableRaisingEvents = true};

            void EventHandler(object s, FileSystemEventArgs e)
            {
                var devDocument = new XmlDocument();
                devDocument.Load(devXMLPath);
                Logger.Log("Heel Development file has been updated!");
                LoadXML(XDocument.Parse(devDocument.OuterXml), true);
            }

            fsWatcher.Changed += EventHandler;
            fsWatcher.EnableRaisingEvents = true;
        }
        catch (Exception e)
        {
            Logger.Log("Tried to load heel Development XML, but failed!");
            Logger.Log(e.ToString());
        }
    }

    private static void LoadXML(XDocument manifestDocument, bool isDevelopment)
    {
        {
            var heelData = manifestDocument?.Root?.Element("AI_HeelsData")?.Elements("heel");
            var guid = manifestDocument?.Root?.Element("guid")?.Value;
            if (heelData != null)
                foreach (var element in heelData)
                {
                    var heelID = int.Parse(element.Attribute("id")?.Value ?? string.Empty);
                    var resolvedID =
                        UniversalAutoResolver.TryGetResolutionInfo(heelID, "ChaFileClothes.ClothesShoes", guid);
                    if (resolvedID != null)
                    {
                        Logger.Log($"Found Resolved ID: \"{heelID}\"=>\"{resolvedID.LocalSlot}\"");
                        heelID = resolvedID.LocalSlot;
                    }

                    Values.Configs.Remove(heelID);
                }

            LoadXML(manifestDocument);
        }
    }

    internal static void LoadXML(XDocument manifestDocument)
    {
        // Load XML and put all Heel Data on plugin's data dictionary.
        var heelData = manifestDocument?.Root?.Element("AI_HeelsData")?.Elements("heel");
        var guid = manifestDocument?.Root?.Element("guid")?.Value;
        if (heelData == null) return;
        Logger.Log($"Registering Heelz Data for \"{guid}\"");
        foreach (var element in heelData)
        {
            var heelID = int.Parse(element.Attribute("id")?.Value);
            Logger.Log($"Registering Heel Config for clothe ID: {heelID}");
            
            if (heelID <= -1) continue;
            var newConfig = new HeelConfig();
            
            Logger.Log("Finding sideloader reference");
            var resolvedID = UniversalAutoResolver.TryGetResolutionInfo(heelID, "ChaFileClothes.ClothesShoes", guid);
            if (resolvedID != null)
            {
                Logger.Log($"Found Resolved ID: \"{heelID}\"=>\"{resolvedID.LocalSlot}\"");
                heelID = resolvedID.LocalSlot;
            }
            else
            { 
                Logger.Log($"Unable to resolve ID: {heelID}.");
            }
                
            if (Values.Configs.ContainsKey(heelID))
            {
                Logger.Log($"CONFLICTING HEEL DATA! Shoe ID {heelID} already has heel data.");
                return;
            }
            
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

                        Logger.Log($"\t{partKey}_{modKey}: {vectors[modKey].ToString()}");

                        if (modKey != "roll") continue;
                        
                        var min = partElement.Element(modKey)?.Attribute("min")?.Value
                            ?.Split(',');
                        var max = partElement.Element(modKey)?.Attribute("max")?.Value
                            ?.Split(',');
                        
                        if (min != null)
                        {
                            vectors.Add(modKey + "min",
                                new Vector3(float.Parse(min[0]), float.Parse(min[1]),
                                    float.Parse(min[2])));
                            Logger.Log($"\t{partKey}_{modKey + "min"}: {vectors[modKey + "min"].ToString()}");
                        }

                        if (max != null)
                        {
                            vectors.Add(modKey + "max",
                                new Vector3(float.Parse(max[0]), float.Parse(max[1]),
                                    float.Parse(max[2])));
                            Logger.Log($"\t{partKey}_{modKey + "max"}: {vectors[modKey + "max"].ToString()}");
                        }
                    }

                    newConfig.heelVectors.Add(partKey, vectors);

                    // register parent angle derivation.
                    var isFixed = partElement?.Attribute("fixed")?.Value;
                    if (isFixed == null) continue;
                    newConfig.isFixed.Add(partKey, bool.Parse(isFixed));
                    Logger.Log($"\t{partKey}_isFixed: {isFixed}");
                }

                var rootSplit = element.Element("root")?.Attribute("vec")?.Value?.Split(',');
                if (rootSplit != null)
                    newConfig.rootMove = new Vector3(float.Parse(rootSplit[0]), float.Parse(rootSplit[1]),
                        float.Parse(rootSplit[2]));
                else
                    newConfig.rootMove = Vector3.zero;

                newConfig.loaded = true;

                Values.Configs.Add(heelID, newConfig);
                Logger.Log($"Registered new heel ID: \"{heelID}\"");
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
            }
        }
    }
}