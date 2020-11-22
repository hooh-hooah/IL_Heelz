using System.Xml.Linq;
using UnityEngine;
using static Heels.Utility.Parser;

namespace Heels.Struct
{
    public struct JointData
    {
        public bool DeriveParent; // Fixed
        public Vector3 Move;
        public Vector3 Roll;
        public Vector3 Scale;
        public Vector3 RollMin;
        public Vector3 RollMax;


        public void LoadPart(XElement partElement)
        {
            Move = GetElementValue(partElement, "move");
            Scale = GetElementValue(partElement, "scale", "vec", Vector3.one);
            Roll = GetElementValue(partElement, "roll");
            RollMin = GetElementValue(partElement, "roll", "min");
            RollMax = GetElementValue(partElement, "roll", "max");
            DeriveParent = partElement?.Attribute("fixed")?.Value != null;
        }

        private static Vector3 GetElementValue(XElement partElement, string key, string valueKey = "vec", Vector3 defValue = default)
        {
            return TryParseFloat(partElement.Element(key)?.Attribute(valueKey)?.Value ?? "", out var v) ? v : defValue;
        }
    }
}