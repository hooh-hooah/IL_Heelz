using CharaCustom;
using UnityEngine;

namespace Heels.Struct
{
    public struct TransformData
    {
        private const string KosiRoot = "BodyTop/p_cf_anim/cf_J_Root/cf_N_height/cf_J_Hips/cf_J_Kosi01/cf_J_Kosi02";
        private const string LeftAnkle = KosiRoot + "/cf_J_LegUp00_L/cf_J_LegLow01_L/cf_J_LegLowRoll_L/cf_J_Foot01_L";
        private const string LeftFoot = LeftAnkle + "/cf_J_Foot02_L";
        private const string LeftToes = LeftFoot + "/cf_J_Toes01_L";
        private const string RightAnkle = KosiRoot + "/cf_J_LegUp00_R/cf_J_LegLow01_R/cf_J_LegLowRoll_R/cf_J_Foot01_R";
        private const string RightFoot = RightAnkle + "/cf_J_Foot02_R";
        private const string RightToes = RightFoot + "/cf_J_Toes01_R";

        public Transform LeftFoot01;
        public Vector3[] LeftFoot01Memory;
        public Transform LeftFoot02;
        public Vector3[] LeftFoot02Memory;
        public Transform LeftToes01;
        public Vector3[] LeftToes01Memory;
        public Transform RightFoot01;
        public Vector3[] RightFoot01Memory;
        public Transform RightFoot02;
        public Vector3[] RightFoot02Memory;
        public Transform RightToes01;
        public Vector3[] RightToes01Memory;

        private static Vector3[] zeroArray = {Vector3.zero, Vector3.zero, Vector3.one};

        public TransformData(Transform root)
        {
            LeftFoot01 = root.Find(LeftAnkle);
            LeftFoot02 = root.Find(LeftFoot);
            LeftToes01 = root.Find(LeftToes);
            RightFoot01 = root.Find(RightAnkle);
            RightFoot02 = root.Find(RightFoot);
            RightToes01 = root.Find(RightToes);
            LeftFoot01Memory = zeroArray;
            LeftFoot02Memory = zeroArray;
            LeftToes01Memory = zeroArray;
            RightFoot01Memory = zeroArray;
            RightFoot02Memory = zeroArray;
            RightToes01Memory = zeroArray;
        }

        public void RememberTransform()
        {
            LeftFoot01Memory = CopyVectors(LeftFoot01.localPosition, LeftFoot01.localEulerAngles, LeftFoot01.localScale);
            LeftFoot02Memory = CopyVectors(LeftFoot02.localPosition, LeftFoot02.localEulerAngles, LeftFoot02.localScale);
            LeftToes01Memory = CopyVectors(LeftToes01.localPosition, LeftToes01.localEulerAngles, LeftToes01.localScale);
            RightFoot01Memory = CopyVectors(RightFoot01.localPosition, RightFoot01.localEulerAngles, RightFoot01.localScale);
            RightFoot02Memory = CopyVectors(RightFoot02.localPosition, RightFoot02.localEulerAngles, RightFoot02.localScale);
            RightToes01Memory = CopyVectors(RightToes01.localPosition, RightToes01.localEulerAngles, RightToes01.localScale);
        }

        public Vector3[] CopyVectors(Vector3 a, Vector3 b, Vector3 c)
        {
            return new[] {a, b, c};
        }

        public void ForgetTransform()
        {
            LeftFoot01Memory = zeroArray;
            LeftFoot02Memory = zeroArray;
            LeftToes01Memory = zeroArray;
            RightFoot01Memory = zeroArray;
            RightFoot02Memory = zeroArray;
            RightToes01Memory = zeroArray;
        }

        public void ApplyTransform(HeelsConfig config, bool isAnimatorActive)
        {
            ApplyTransformData(LeftFoot01, LeftFoot01Memory, config.Ankle, isAnimatorActive);
            ApplyTransformData(RightFoot01, RightFoot01Memory, config.Ankle, isAnimatorActive);
            ApplyTransformData(LeftFoot02, LeftFoot02Memory, config.Foot, isAnimatorActive);
            ApplyTransformData(RightFoot02, RightFoot02Memory, config.Foot, isAnimatorActive);
            ApplyTransformData(LeftToes01, LeftToes01Memory, config.Toes, isAnimatorActive);
            ApplyTransformData(RightToes01, RightToes01Memory, config.Toes, isAnimatorActive);
        }

        public static void ApplyTransformData(Transform target, Vector3[] memory, JointData data, bool isAnimatorActive)
        {
            if (data.DeriveParent) target.eulerAngles = target.parent.eulerAngles;
            // only update when body animator is actually doing something.
            if (!isAnimatorActive) return;

            target.localPosition += data.Move; // calculate from animation and abmx.
            target.localScale = data.Scale; // calculate from abmx

            var anchorPosition = target.position;
            var isValidAngleLimit = data.RollMin != Vector3.zero || data.RollMax != Vector3.zero;
            target.RotateAround(anchorPosition, target.right,
                isValidAngleLimit ? Mathf.Clamp(data.Roll.x, data.RollMin.x, data.RollMax.x) : data.Roll.x);
            target.RotateAround(anchorPosition, target.up,
                isValidAngleLimit ? Mathf.Clamp(data.Roll.y, data.RollMin.y, data.RollMax.y) : data.Roll.y);
            target.RotateAround(anchorPosition, target.forward,
                isValidAngleLimit ? Mathf.Clamp(data.Roll.z, data.RollMin.z, data.RollMax.z) : data.Roll.z);
        }
    }
}