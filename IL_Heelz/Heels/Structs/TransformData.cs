using CharaCustom;
using Heels.Handler;
using System;
using System.Linq;
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
        public const string LeftHandEffectorName = "f_t_arm_L";
        public const string RightHandEffectorName = "f_t_arm_R";
        private const string LeftFootEffectorName = "f_t_leg_L";
        private const string RightFootEffectorName = "f_t_leg_R";
        public const string LeftHandHintEffectorName = "f_t_elbo_L";
        public const string RightHandHintEffectorName = "f_t_elbo_R";

        public Transform Root;
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
        public Transform LeftHandEffector;
        public Transform RightHandEffector;
        public Transform LeftFootEffector;
        public Transform RightFootEffector;
        public Transform LeftHandHintEffector;
        public Transform RightHandHintEffector;
#if HS2
        public Illusion.Component.Correct.BaseData LeftHandBaseData;
        public Illusion.Component.Correct.BaseData RightHandBaseData;
        public Illusion.Component.Correct.BaseData LeftFootBaseData;
        public Illusion.Component.Correct.BaseData RightFootBaseData;
#else
        public Correct.BaseData LeftHandBaseData;
        public Correct.BaseData RightHandBaseData;
        public Correct.BaseData LeftFootBaseData;
        public Correct.BaseData RightFootBaseData;
#endif


        private static Vector3[] zeroArray = {Vector3.zero, Vector3.zero, Vector3.one};

        public TransformData(Transform root)
        {
            Root = root;
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
            LeftHandEffector = root.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(LeftHandEffectorName)).FirstOrDefault();
            RightHandEffector = root.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(RightHandEffectorName)).FirstOrDefault();
            LeftFootEffector = root.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(LeftFootEffectorName)).FirstOrDefault();
            RightFootEffector = root.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(RightFootEffectorName)).FirstOrDefault();
            LeftHandHintEffector = root.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(LeftHandHintEffectorName)).FirstOrDefault();
            RightHandHintEffector = root.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(RightHandHintEffectorName)).FirstOrDefault();

#if HS2
            LeftHandBaseData = root.GetComponentsInChildren<Illusion.Component.Correct.BaseData>().Where(x => x.name.Contains(LeftHandEffectorName)).FirstOrDefault();
            RightHandBaseData = root.GetComponentsInChildren<Illusion.Component.Correct.BaseData>().Where(x => x.name.Contains(RightHandEffectorName)).FirstOrDefault();
            LeftFootBaseData = root.GetComponentsInChildren<Illusion.Component.Correct.BaseData>().Where(x => x.name.Contains(LeftFootEffectorName)).FirstOrDefault();
            RightFootBaseData = root.GetComponentsInChildren<Illusion.Component.Correct.BaseData>().Where(x => x.name.Contains(RightFootEffectorName)).FirstOrDefault();

#else
            LeftHandBaseData = root.GetComponentsInChildren<Correct.BaseData>().Where(x => x.name.Contains(LeftHandEffectorName)).FirstOrDefault();
            RightHandBaseData = root.GetComponentsInChildren<Correct.BaseData>().Where(x => x.name.Contains(RightHandEffectorName)).FirstOrDefault();
            LeftFootBaseData = root.GetComponentsInChildren<Correct.BaseData>().Where(x => x.name.Contains(LeftFootEffectorName)).FirstOrDefault();
            RightFootBaseData = root.GetComponentsInChildren<Correct.BaseData>().Where(x => x.name.Contains(RightFootEffectorName)).FirstOrDefault();
#endif
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

        public void ApplyTransform(HeelsConfig config, bool isAnimatorActive, bool updateLeft, bool updateRight)
        {
            if (updateLeft)
            {
                ApplyTransformData(LeftFoot01, LeftFoot01Memory, config.Ankle, isAnimatorActive);
                ApplyTransformData(LeftFoot02, LeftFoot02Memory, config.Foot, isAnimatorActive);
                ApplyTransformData(LeftToes01, LeftToes01Memory, config.Toes, isAnimatorActive);
            }

            if (updateRight)
            {
                ApplyTransformData(RightFoot01, RightFoot01Memory, config.Ankle, isAnimatorActive);
                ApplyTransformData(RightFoot02, RightFoot02Memory, config.Foot, isAnimatorActive);
                ApplyTransformData(RightToes01, RightToes01Memory, config.Toes, isAnimatorActive);
            }
        }

        public static void ApplyTransformData(Transform target, Vector3[] memory, JointData data, bool isAnimatorActive)
        {
            if (data.DeriveParent) target.eulerAngles = target.parent.eulerAngles;
            // only update when body animator is actually doing something.
            if (!isAnimatorActive) return;

            target.localPosition += data.Move; // calculate from animation and abmx.
//            target.localScale = data.Scale; // calculate from abmx

            var anchorPosition = target.position;
            var isValidAngleLimit = data.RollMin != Vector3.zero || data.RollMax != Vector3.zero;
            target.RotateAround(anchorPosition, target.right,
                isValidAngleLimit ? Mathf.Clamp(data.Roll.x, data.RollMin.x, data.RollMax.x) : data.Roll.x);
            target.RotateAround(anchorPosition, target.up,
                isValidAngleLimit ? Mathf.Clamp(data.Roll.y, data.RollMin.y, data.RollMax.y) : data.Roll.y);
            target.RotateAround(anchorPosition, target.forward,
                isValidAngleLimit ? Mathf.Clamp(data.Roll.z, data.RollMin.z, data.RollMax.z) : data.Roll.z);
        }

  /*      public void ApplyFeetEffectors(HeelsConfig config, bool isAnimatorActive)
        {
            if (!isAnimatorActive || LeftFootEffector == null || RightFootEffector == null)
                return;

            LeftFootEffector.position += config.Root;
            RightFootEffector.position += config.Root;
        }
  */
        public void ApplyLeftFootEffector(Vector3 offset, bool isAnimatorActive, HeelsHandler.AnimationType animationType)
        {
            if (!isAnimatorActive || LeftFootEffector == null || LeftFootBaseData?.bone == null)
                return;


            if (animationType == HeelsHandler.AnimationType.FeetForward)
            {
                LeftFootEffector.position += offset.y * LeftFoot02.forward;
                LeftFootBaseData.bone.position += offset.y * LeftFoot02.forward;
                LeftFootEffector.position += offset.y * Root.up;
                LeftFootBaseData.bone.position += offset.y * Root.up;
            }
            else if (animationType == HeelsHandler.AnimationType.FeetBackward)
            {
                LeftFootEffector.position -= offset.y * LeftFoot02.forward;
                LeftFootBaseData.bone.position -= offset.y * LeftFoot02.forward;
                LeftFootEffector.position += offset.y * Root.up;
                LeftFootBaseData.bone.position += offset.y * Root.up;
            }
            else
            {
                LeftFootEffector.position += offset.y * LeftFootEffector.up;
                LeftFootBaseData.bone.position += offset.y * LeftFootEffector.up;
            }
        }

        public void ApplyRightFootEffector(Vector3 offset, bool isAnimatorActive, HeelsHandler.AnimationType animationType)
        {
            if (!isAnimatorActive || RightFootEffector == null || RightFootBaseData?.bone == null)
                return;

            if (animationType == HeelsHandler.AnimationType.FeetForward)
            {
                RightFootEffector.position += offset.y * RightFoot02.forward;
                RightFootBaseData.bone.position += offset.y * RightFoot02.forward;
                RightFootEffector.position += offset.y * Root.up;
                RightFootBaseData.bone.position += offset.y * Root.up;
            }
            else if (animationType == HeelsHandler.AnimationType.FeetBackward)
            {
                RightFootEffector.position -= offset.y * RightFoot02.forward;
                RightFootBaseData.bone.position -= offset.y * RightFoot02.forward;
                RightFootEffector.position += offset.y * Root.up;
                RightFootBaseData.bone.position += offset.y * Root.up;
            }
            else
            {
                RightFootEffector.position += offset.y * RightFootEffector.up;
                RightFootBaseData.bone.position += offset.y * RightFootEffector.up;
            }
        }

        public void ApplyLeftFootEffectorOffset(Vector3 offset, bool isAnimatorActive)
        {
            if (!isAnimatorActive || LeftFootEffector == null || LeftFootBaseData?.bone == null)
                return;

            LeftFootEffector.localPosition += offset;
            LeftFootBaseData.bone.localPosition += offset;
        }

        public void ApplyRightFootEffectorOffset(Vector3 offset, bool isAnimatorActive)
        {
            if (!isAnimatorActive || RightFootEffector == null || RightFootBaseData?.bone == null)
                return;

            RightFootEffector.localPosition += offset;
            RightFootBaseData.bone.localPosition += offset;
        }

        public void ApplyLeftHandEffector(Vector3 offset, bool isAnimatorActive, bool standHover)
        {
            if (!isAnimatorActive || LeftHandEffector == null || LeftHandBaseData?.bone == null)
                return;

            if (standHover)
            {
                LeftHandEffector.position -= offset;
                LeftHandBaseData.bone.position -= offset;
            }
            else
            {
                LeftHandEffector.position += offset / 2;
                LeftHandBaseData.bone.position += offset / 2;
            }
        }

        public void ApplyRightHandEffector(Vector3 offset, bool isAnimatorActive, bool standHover)
        {
            if (!isAnimatorActive || RightHandEffector == null || RightHandBaseData?.bone == null)
                return;

            if (standHover)
            {
                RightHandEffector.position -= offset;
                RightHandBaseData.bone.position -= offset;
            }
            else
            {
                RightHandEffector.position += offset / 2;
                RightHandBaseData.bone.position += offset / 2;
            }
        }

        public void SetLeftHandEffector(Vector3 position, bool isAnimatorActive)
        {
            if (!isAnimatorActive || LeftHandEffector == null || LeftHandBaseData?.bone == null)
                return;

            LeftHandEffector.position = position;
            LeftHandBaseData.bone.position = position;
        }

        public void SetRightHandEffector(Vector3 position, bool isAnimatorActive)
        {
            if (!isAnimatorActive || RightHandEffector == null || RightHandBaseData?.bone == null)
                return;

            RightHandEffector.position = position;
            RightHandBaseData.bone.position = position;
        }

        public void ApplyLeftHandEffectorWithHint(Vector3 offset, Vector3 hintOffset, bool isAnimatorActive)
        {
            if (!isAnimatorActive || LeftHandEffector == null || LeftHandHintEffector == null || LeftHandBaseData?.bone == null)
                return;

            LeftHandEffector.localPosition += offset;
            LeftHandBaseData.bone.localPosition += offset;
            LeftHandHintEffector.localPosition += hintOffset;
        }

        public void ApplyRightHandEffectorWithHint(Vector3 offset, Vector3 hintOffset, bool isAnimatorActive)
        {
            if (!isAnimatorActive || RightHandEffector == null || RightHandHintEffector == null || RightHandBaseData?.bone == null)
                return;

            RightHandEffector.localPosition += offset;
            RightHandBaseData.bone.localPosition += offset;
            RightHandHintEffector.localPosition += hintOffset;
        }
    }
}