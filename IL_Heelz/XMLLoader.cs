using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Heels;
using Heels.Struct;
using Sideloader.AutoResolver;
using UnityEngine;
using Logger = Util.Log.Logger;

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
            var heelID = int.Parse(element.Attribute("id")?.Value ?? "-1");
            Logger.Log($"Registering Heel Config for clothe ID: {heelID}");

            if (heelID <= -1) continue;

            Logger.Log("Finding sideloader reference");
            var resolvedID = UniversalAutoResolver.TryGetResolutionInfo(heelID, "ChaFileClothes.ClothesShoes", guid);
            if (resolvedID != null)
            {
                Logger.Log($"Found Resolved ID: \"{heelID}\"=>\"{resolvedID.LocalSlot}\"");
                heelID = resolvedID.LocalSlot;
            }
            else
            {
                // Due to some limitation, I'm limiting heels registration to the sideloader items. 
                Logger.Log($"Unable to resolve ID: {heelID}.");
                return;
            }

            if (Values.Configs.ContainsKey(heelID))
            {
                Logger.Log($"CONFLICTING HEEL DATA! Shoe ID {heelID} already has heel data.");
                return;
            }

            try
            {
                var newConfig = new HeelsConfig(element);

                if (heelID <= 0)
                {
                    Logger.Log($"Heelz refused to register heel ID: \"{heelID}\"");
                }
                else
                {
                    Values.Configs.Add(heelID, newConfig);
                    Logger.Log($"Registered new heel ID: \"{heelID}\"");
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
            }
        }
    }
}