using System.Xml.Linq;
using UnityEngine;
using static Heels.Utility.Parser;

namespace Heels.Struct
{
    public struct HeelsConfig
    {
        public bool Loaded;
        public Vector3 Root;
        public JointData Ankle; // foot01
        public JointData Foot; // foot02
        public JointData Toes; // toes01

        public static string[] parts = {"foot01", "foot02", "toes01"};

        public HeelsConfig(XElement element)
        {
            Loaded = true;
            Root = Vector3.zero;
            Ankle = new JointData();
            Foot = new JointData();
            Toes = new JointData();

            foreach (var partKey in parts)
            {
                var partElement = element.Element(partKey);
                if (partElement == null)
                    continue;

                switch (partKey)
                {
                    case "foot01":
                        Ankle.LoadPart(partElement);
                        break;
                    case "foot02":
                        Foot.LoadPart(partElement);
                        break;
                    case "toes01":
                        Toes.LoadPart(partElement);
                        break;
                }
            }

            Root = TryParseFloat(element.Element("root")?.Attribute("vec")?.Value, out var rootVector) ? rootVector : Vector3.zero;
        }
    }
}