using System;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Heels.Struct;
using RootMotion.FinalIK;
using UnityEngine;

namespace Heels.Handler
{
    public class HeelsHandler
    {
        public enum AnimationType
        {
            Standing,  // Apply hover
            Sitting,   // Only hover feet
            Prone      // Do nothing
        };

        public enum LimbHoverType
        {
            None,      // Do not adjust limb when hovering
            Right, // Only adjust right limb
            Left,  // Only adjust left limb
            Both  // adjust both limbs
        };

        public bool delegateRegistered;
        public bool IsActive;

        public bool IsHover = false;
 //       public bool IsFootHover = false;

        public HeelsHandler(ChaControl chaControl)
        {
            ChaControl = chaControl;
            GameObject = chaControl.gameObject;
            Transform = GameObject.transform;
            ChildTransform = Transform.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("cf_N_height")).FirstOrDefault();
            TargetTransforms = new TransformData(Transform);
            IKSolver = ChaControl.fullBodyIK.solver;
            currentAnimationType = AnimationType.Standing;
        }

        public IKSolverFullBodyBiped IKSolver { get; }

        public TransformData TargetTransforms { get; }

        public Transform ChildTransform { get; }

        public Transform Transform { get; }

        public GameObject GameObject { get; }

        public ChaControl ChaControl { get; }

        public HeelsConfig Config { get; set; }

        public bool CanUpdate => IsActive &&
                                 ChaControl != null &&
                                 ChaControl.fileStatus != null &&
                                 ChaControl.fileStatus.clothesState[Constant.ShoeCategory] == 0;

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

        public void UpdateStatus()
        {
            Console.WriteLine($"UpdateStatus: {ChaControl.name}");

            if (Manager.HSceneManager.isHScene)
            {
                Console.WriteLine($"isHScene");
                Console.WriteLine($"IsHover: {IsHover}");
 //               Console.WriteLine($"IsFootHover: {IsFootHover}");
                if (IsHover)
                    HoverBody(false);

//                if (IsFootHover)
//                    HoverFeet(false);

                return;
            }

            HoverBody(CanUpdate && currentAnimationType == AnimationType.Standing);
 //           HoverFeet(CanUpdate && currentAnimationType == AnimationType.Sitting);
        }

        public void UpdateAnimation(string animationName)
        {
            currentAnimationType = AnimationType.Standing;
            currentHandHoverType = LimbHoverType.None;
            currentFootHoverType = LimbHoverType.None;

            string assetBundlePath = $"list\\heels\\heels.unity3d";
            string assetName = ChaControl.animBody.runtimeAnimatorController.name;

            Console.WriteLine($"UpdateAnimation: {ChaControl.name} {assetName} {animationName}");
            // change the hover behavior based on the game state.
            if (!AssetBundleCheck.IsFile(assetBundlePath, assetName))
            {
                Console.WriteLine($"Could not locate {assetBundlePath}");
                return;
            }
         
            AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = AssetBundleManager.LoadAsset(assetBundlePath, assetName, typeof(ExcelData), null);
            AssetBundleManager.UnloadAssetBundle(assetBundlePath, true, null, false);
            if (assetBundleLoadAssetOperation.IsEmpty())
            {
                Console.WriteLine($"assetBundleLoadAssetOperation is empty");
                return;
            }

            ExcelData asset = assetBundleLoadAssetOperation.GetAsset<ExcelData>();
            int num = asset.MaxCell - 1;
            int row = asset.list[num].list.Count - 1;
            List<ExcelData.Param> assetList = asset.Get(new ExcelData.Specify(0, 0), new ExcelData.Specify(num, row));

            if (assetList.IsNullOrEmpty())
            {
                Console.WriteLine($"assetList is empty");
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
                Console.WriteLine($"assetItem Not Found");
                return;
            }

            if (Int32.TryParse(assetItem[1], out int result))
                currentAnimationType = (AnimationType)result;

            if (Int32.TryParse(assetItem[2], out result))
                currentHandHoverType = (LimbHoverType)result;

            if (Int32.TryParse(assetItem[3], out result))
                currentFootHoverType = (LimbHoverType)result;

            Console.WriteLine($"currentAnimationType: {currentAnimationType} {currentHandHoverType} {currentFootHoverType}");

            UpdateStatus();
        }

        public void UpdateFootAngle()
        {
            if (!CanUpdate) 
                return;

            TargetTransforms.ApplyTransform(Config, ChaControl.animBody.isActiveAndEnabled);
        }

        public void UpdateEffectors()
        {
            if (!CanUpdate)
                return;

            UpdateFootEffectors(Manager.HSceneManager.isHScene);
            UpdateHandEffectors(Manager.HSceneManager.isHScene);
        }

 /*       public void UpdateFootEffectors()
        {
            if (!IsFootHover)
                return;

            TargetTransforms.ApplyFeetEffectors(Config, ChaControl.animBody.isActiveAndEnabled);
        }
 */
        public void UpdateFootEffectors(bool forceOn)
        {
            if (forceOn || currentFootHoverType == LimbHoverType.Left || currentFootHoverType == LimbHoverType.Both)
                TargetTransforms.ApplyLeftFootEffector(Config, ChaControl.animBody.isActiveAndEnabled);

            if (forceOn || currentFootHoverType == LimbHoverType.Right || currentFootHoverType == LimbHoverType.Both)
                TargetTransforms.ApplyRightFootEffector(Config, ChaControl.animBody.isActiveAndEnabled);
        }

        public void UpdateHandEffectors(bool forceOff)
        {
            if (forceOff)
                return;

            if (currentHandHoverType == LimbHoverType.Left || currentHandHoverType == LimbHoverType.Both)
                TargetTransforms.ApplyLeftHandEffector(Config, ChaControl.animBody.isActiveAndEnabled, currentAnimationType == AnimationType.Standing);

            if (currentHandHoverType == LimbHoverType.Right || currentHandHoverType == LimbHoverType.Both)
                TargetTransforms.ApplyRightHandEffector(Config, ChaControl.animBody.isActiveAndEnabled, currentAnimationType == AnimationType.Standing);
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

            ChildTransform.localPosition = !IsHover ? Vector3.zero : new Vector3(0, Config.Root.y, 0);
        }

 //       public void HoverFeet(bool hover)
 //       {
 //           IsFootHover = hover;
 //       }

        public void Reset()
        {
            if (ChaControl == null) 
                return;

            IsHover = false;
 //           IsFootHover = false;

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