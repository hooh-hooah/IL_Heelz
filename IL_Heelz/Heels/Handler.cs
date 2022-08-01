using System;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Heels.Struct;
using RootMotion.FinalIK;
using UnityEngine;
using Heelz;

namespace Heels.Handler
{
    public class HeelsHandler
    {
        public enum AnimationType
        {
            Standing,      // Apply hover
            FeetForward,   // Hover feet up and forward
            FeetBackward,  // Hover feet up and backward
            FeetUpward,    // Hover feet up
            Carried        // Do nothing, don't rotate feet
        };

        public enum LimbHoverType
        {
            None,      // Do not adjust limb when hovering
            Right,     // Only adjust right limb
            Left,      // Only adjust left limb
            Both      // adjust both limbs
        };

        public bool delegateRegistered;
        public bool IsActive;

        public bool IsHover = false;

        public HeelsHandler(ChaControl chaControl)
        {
            ChaControl = chaControl;
            GameObject = chaControl.gameObject;
            Transform = GameObject.transform;
            ChildTransform = Transform.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("cf_N_height")).FirstOrDefault();
            HSceneTransform = Transform.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("BodyTop")).FirstOrDefault();
            TargetTransforms = new TransformData(Transform);
            IKSolver = ChaControl.fullBodyIK.solver;
            currentAnimationType = AnimationType.Standing;
            isHScene = false;
        }

        public IKSolverFullBodyBiped IKSolver { get; }

        public TransformData TargetTransforms { get; }

        public Transform ChildTransform { get; set; }

        public Transform HSceneTransform { get; set; }

        public Transform Transform { get; }

        public GameObject GameObject { get; }

        public ChaControl ChaControl { get; }

        public HeelsConfig Config { get; set; }

        public bool CanUpdate => IsActive &&
                                 ChaControl != null &&
                                 ChaControl.fileStatus != null &&
                                 ChaControl.fileStatus.clothesState[Constant.ShoeCategory] == 0;

        public bool CanOffset => ChaControl != null &&
                                 ChaControl.fileParam != null &&
                                 !Manager.HSceneManager.isHScene;

        public bool isHScene { get; set; }

        public AnimationType currentAnimationType { get; set; }

        public LimbHoverType currentHandHoverType { get; set; }

        public LimbHoverType currentFootHoverType { get; set; }


        public void OnEnabled()
        {
            // register job
            // remember foot transform
            TargetTransforms.RememberTransform();
        }

        public void OnDisabled()
        {
            // undo job
            // forget foot transform
            TargetTransforms.ForgetTransform();
        }

        public void ResetBodyPosition()
        {
            if (isHScene)
            {
                if (ChildTransform == null)
                    ChildTransform = Transform?.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("cf_N_height")).FirstOrDefault();

                if (ChildTransform == null)
                    return;

                ChildTransform.localPosition = Vector3.zero;
            }
            else
            {
                if (HSceneTransform == null)
                    HSceneTransform = Transform.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("BodyTop")).FirstOrDefault();

                if (HSceneTransform == null)
                    return;

                HSceneTransform.localPosition = Vector3.zero;
            }
        }

        public void SetBodyPosition(Vector3 position)
        {
            if (isHScene)
            {
                if (HSceneTransform == null)
                    HSceneTransform = Transform.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("BodyTop")).FirstOrDefault();

                if (HSceneTransform == null)
                    return;

                HSceneTransform.localPosition = position;
            }
            else
            {
                if (ChildTransform == null)
                    ChildTransform = Transform?.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("cf_N_height")).FirstOrDefault();

                if (ChildTransform == null)
                    return;

                ChildTransform.localPosition = position;
            }
        }

        public void UpdateStatus()
        {
            if (isHScene != Manager.HSceneManager.isHScene)
            {
                isHScene = Manager.HSceneManager.isHScene;
                ResetBodyPosition();
            }

            HoverBody(CanUpdate && currentAnimationType == AnimationType.Standing);
        }

        public void UpdateAnimation(string animationName)
        {
            if (ChaControl.fileParam.sex == 0 || ChaControl?.animBody?.runtimeAnimatorController == null)
                return;

            currentAnimationType = isHScene ? AnimationType.FeetUpward :AnimationType.Standing;
            currentHandHoverType = LimbHoverType.None;
            currentFootHoverType = isHScene ? LimbHoverType.Both : LimbHoverType.None;

            if (isHScene && HeelzPlugin.simpleHeelsInH.Value)
                return;

            string assetBundlePath = $"list\\heels\\heels.unity3d";
            string assetName = ChaControl.animBody.runtimeAnimatorController.name;

            Debug.Log($"UpdateAnimation: {ChaControl.name} {assetName} {animationName}");
            // change the hover behavior based on the game state.
            if (!AssetBundleCheck.IsFile(assetBundlePath, assetName))
            {
                Debug.Log($"Could not locate {assetBundlePath}");
                return;
            }
         
            AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = AssetBundleManager.LoadAsset(assetBundlePath, assetName, typeof(ExcelData), null);
            AssetBundleManager.UnloadAssetBundle(assetBundlePath, true, null, false);
            if (assetBundleLoadAssetOperation.IsEmpty())
            {
                Debug.Log($"assetBundleLoadAssetOperation is empty");
                return;
            }

            ExcelData asset = assetBundleLoadAssetOperation.GetAsset<ExcelData>();
            int num = asset.MaxCell - 1;
            int row = asset.list[num].list.Count - 1;
            List<ExcelData.Param> assetList = asset.Get(new ExcelData.Specify(0, 0), new ExcelData.Specify(num, row));

            if (assetList.IsNullOrEmpty())
            {
                Debug.Log($"assetList is empty");
                return;
            }

            bool assetItemFound = false;
            List<string> assetItem = new List<string>();

            foreach (var excelItem in assetList)
            {
                if (excelItem.list[0] == animationName)
                {
                    assetItemFound = true;
                    assetItem = excelItem.list;
                    break;
                }
            }
            
            if (assetItem == null || !assetItemFound)
            {
                Debug.Log($"assetItem Not Found");
                return;
            }

            if (Int32.TryParse(assetItem[1], out int result))
                currentAnimationType = (AnimationType)result;

            if (Int32.TryParse(assetItem[2], out result))
                currentHandHoverType = (LimbHoverType)result;

            if (Int32.TryParse(assetItem[3], out result))
                currentFootHoverType = (LimbHoverType)result;

            if (!isHScene && HeelzPlugin.simpleHeelsInWorld.Value && currentAnimationType != AnimationType.Standing)
            {
                currentAnimationType = AnimationType.FeetUpward;
                currentHandHoverType = LimbHoverType.None;
                currentFootHoverType = LimbHoverType.Both;
            }

            Debug.Log($"currentAnimationType: {currentAnimationType} {currentHandHoverType} {currentFootHoverType}");

            UpdateStatus();
        }

        public void UpdateFootAngle()
        {
            if (!CanUpdate) 
                return;

            bool updateLeftAnkle = HeelzPlugin.alwaysRotateHeels.Value || currentAnimationType == AnimationType.Standing || currentFootHoverType == LimbHoverType.Left || currentFootHoverType == LimbHoverType.Both;
            bool updateRightAnkle = HeelzPlugin.alwaysRotateHeels.Value || currentAnimationType == AnimationType.Standing || currentFootHoverType == LimbHoverType.Right || currentFootHoverType == LimbHoverType.Both;

            TargetTransforms.ApplyTransform(Config, ChaControl.animBody.isActiveAndEnabled, updateLeftAnkle, updateRightAnkle);
        }

        public void UpdateEffectors()
        {
            if (!CanUpdate)
                return;

            UpdateFootEffectors();
            UpdateHandEffectors();
        }

        public void UpdateFootEffectors()
        {
            if (currentFootHoverType == LimbHoverType.Left || currentFootHoverType == LimbHoverType.Both)
                TargetTransforms.ApplyLeftFootEffector(Config.Root, ChaControl.animBody.isActiveAndEnabled, currentAnimationType);

            if (currentFootHoverType == LimbHoverType.Right || currentFootHoverType == LimbHoverType.Both)
                TargetTransforms.ApplyRightFootEffector(Config.Root, ChaControl.animBody.isActiveAndEnabled, currentAnimationType);
        }

        public void UpdateHandEffectors()
        {
            if (currentHandHoverType == LimbHoverType.Left || currentHandHoverType == LimbHoverType.Both)
                TargetTransforms.ApplyLeftHandEffector(Config.Root, ChaControl.animBody.isActiveAndEnabled, currentAnimationType == AnimationType.Standing);

            if (currentHandHoverType == LimbHoverType.Right || currentHandHoverType == LimbHoverType.Both)
                TargetTransforms.ApplyRightHandEffector(Config.Root, ChaControl.animBody.isActiveAndEnabled, currentAnimationType == AnimationType.Standing);
        }

        public void SetConfig(HeelsConfig shoeConfig)
        {
            Config = shoeConfig;
            IsActive = true;
            OnEnabled();
            Reset();
        }

        public void SetConfig()
        {
            Config = default;
            IsActive = false;
            OnDisabled();
            Reset();
        }

        public void HoverBody(bool hover)
        {
            IsHover = hover;
            SetBodyPosition(!IsHover ? Vector3.zero : new Vector3(0, Config.Root.y, 0));
        }

        public void Reset()
        {
            if (ChaControl == null) 
                return;

            IsHover = false;

            UpdateStatus();

            if (IsActive && delegateRegistered == false)
            {
                IKSolver.OnPostUpdate = (IKSolver.UpdateDelegate) Delegate.Combine(IKSolver.OnPostUpdate, new IKSolver.UpdateDelegate(UpdateFootAngle));
                delegateRegistered = true;
            }
            else if (!IsActive && delegateRegistered)
            {
                IKSolver.OnPostUpdate = (IKSolver.UpdateDelegate) Delegate.Remove(IKSolver.OnPostUpdate, new IKSolver.UpdateDelegate(UpdateFootAngle));
                delegateRegistered = false;
            }
        }
    }
}