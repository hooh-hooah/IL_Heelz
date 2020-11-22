using UnityEngine;

namespace Heels.Utility
{
    public static class Parser
    {
        public static bool TryParseFloat(string input, out Vector3 output)
        {
            output = Vector3.zero;

            try
            {
                var split = input.Split(',');
                if (split.Length < 3) return false;
                output = new Vector3(float.Parse(split[0]), float.Parse(split[1]),
                    float.Parse(split[2]));
                return true;
            }
            catch
            {
                // something went wrong?
            }

            return false;
        }
    }
}