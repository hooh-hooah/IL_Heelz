using System;
using System.Collections.Generic;
using KKAPI;
using KKAPI.Chara;
using RootMotion.FinalIK;
using UnityEngine;

namespace HeelzCore
{
    public class HeelsController : CharaCustomFunctionController
    {
        private readonly Dictionary<Transform, bool> parentDerivation = new Dictionary<Transform, bool>();
        private readonly Dictionary<Transform, Vector3[]> transformVectors = new Dictionary<Transform, Vector3[]>();
        public HeelConfig currentConfig;
        private bool hoverSwitch;

        public bool GroundAnim { get; set; }

        private bool isShoeActive => ChaControl.fileStatus.clothesState[Constant.shoeCategory] == 0;

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            SetUpShoes();
        }

        public void SetUpShoes()
        {
            if (ChaControl == null || !isShoeActive)
                return;

            var shoeID = ChaControl.nowCoordinate.clothes.parts[Constant.shoeCategory].id;
            HeelConfig shoeConfig;

            Util.Logger.Log(string.Format("Looking for ID: \"{0}\"", shoeID));
            if (Values.configs.TryGetValue(shoeID, out shoeConfig) && shoeConfig.loaded)
            {
                Util.Logger.Log("Found! Removing Transforms..");
                RemoveLocalTransforms();
                Util.Logger.Log("Found! Setting up Transforms..");
                LocalTransforms(shoeConfig);
                EnableHover();
            }
            else
            {
                Util.Logger.Log("Not Found! Removing Transforms..");
                RemoveLocalTransforms();
            }
        }

        public void DisableHover()
        {
            if (hoverSwitch == false)
                return;
            var instanceTransform = ChaControl.gameObject.transform;
            var childTransform = instanceTransform.GetChild(0);
            childTransform.localPosition = Vector3.zero;
            hoverSwitch = false;
        }

        public void EnableHover()
        {
            // TODO: Unfuck this.
            //if (hoverSwitch)
            //    return;
            var instanceTransform = ChaControl.gameObject.transform;
            var childTransform = instanceTransform.GetChild(0);
            if (currentConfig != null)
            {
                childTransform.localPosition = new Vector3(0, currentConfig.rootMove.y, 0);
                hoverSwitch = true;
            }
        }


        public void UpdateHover()
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio)
            {
                if (!isShoeActive || currentConfig == null || !GroundAnim)
                    DisableHover();
                else
                    EnableHover();
            }
            else
            {
                if (!isShoeActive || currentConfig == null)
                    DisableHover();
                else
                    EnableHover();
            }
        }

        public void RemoveLocalTransforms()
        {
            if (ChaControl == null)
                return;
            currentConfig = null;

            DisableHover();
            transformVectors.Clear();
            parentDerivation.Clear();

            var ik = ChaControl.fullBodyIK.solver;
            ik.OnPostUpdate = (IKSolver.UpdateDelegate) Delegate.Remove(ik.OnPostUpdate, new IKSolver.UpdateDelegate(IKArray));
        }

        public void LocalTransforms(HeelConfig heelConfig)
        {
            if (ChaControl == null)
                return;
            currentConfig = heelConfig;

            foreach (var pair in Constant.pathMaps)
            {
                Dictionary<string, Vector3> partVectors;
                heelConfig.heelVectors.TryGetValue(pair.Value, out partVectors);

                if (partVectors != null)
                {
                    Vector3 roll, move, scale, rollMin, rollMax;
                    var boneTransform = ChaControl.gameObject?.transform?.Find(pair.Key);

                    if (boneTransform != null &&
                        partVectors.TryGetValue("roll", out roll) &&
                        partVectors.TryGetValue("move", out move) &&
                        partVectors.TryGetValue("scale", out scale))
                    {
                        partVectors.TryGetValue("rollmin", out rollMin);
                        partVectors.TryGetValue("rollmax", out rollMax);

                        transformVectors.Add(boneTransform, new Vector3[5] {move, roll, scale, rollMin, rollMax});
                    }

                    bool parentDerive;
                    if (heelConfig.isFixed.TryGetValue(pair.Value, out parentDerive))
                        parentDerivation.Add(boneTransform, parentDerive);
                }
            }

            var ik = ChaControl.fullBodyIK.solver;
            ik.OnPostUpdate = (IKSolver.UpdateDelegate) Delegate.Combine(ik.OnPostUpdate, new IKSolver.UpdateDelegate(IKArray));
        }

        public void IKArray()
        {
            if (!isShoeActive)
                return;

            foreach (var macros in transformVectors)
            {
                var targetTransform = macros.Key;

                targetTransform.localPosition += macros.Value[0];
                targetTransform.localScale = macros.Value[2];

                if (parentDerivation.ContainsKey(targetTransform) && parentDerivation[targetTransform])
                    targetTransform.eulerAngles = targetTransform.parent.eulerAngles;

                var anchorPosition = targetTransform.position;
                var isValidAngleLimit = macros.Value[3] != Vector3.zero || macros.Value[4] != Vector3.zero;

                targetTransform.RotateAround(anchorPosition, targetTransform.right,
                    isValidAngleLimit ? Mathf.Clamp(macros.Value[1].x, macros.Value[3].x, macros.Value[4].x) : macros.Value[1].x);
                targetTransform.RotateAround(anchorPosition, targetTransform.up,
                    isValidAngleLimit ? Mathf.Clamp(macros.Value[1].y, macros.Value[3].y, macros.Value[4].y) : macros.Value[1].y);
                targetTransform.RotateAround(anchorPosition, targetTransform.forward,
                    isValidAngleLimit ? Mathf.Clamp(macros.Value[1].z, macros.Value[3].z, macros.Value[4].z) : macros.Value[1].z);
            }
        }
    }
}