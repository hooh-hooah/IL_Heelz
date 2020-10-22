using System;
using System.Collections.Generic;
using KKAPI;
using KKAPI.Chara;
using RootMotion.FinalIK;
using UnityEngine;
using Logger = Util.Logger;

// TODO: Heelz dev tool integration
public class HeelsController : CharaCustomFunctionController
{
    private readonly Dictionary<Transform, bool> _parentDerivation = new Dictionary<Transform, bool>();
    private readonly Dictionary<Transform, Vector3[]> _transformVectors = new Dictionary<Transform, Vector3[]>();
    private HeelConfig _currentConfig;
    private bool _hoverSwitch;

    public bool GroundAnim { get; set; }

    private bool IsShoeActive => ChaControl.fileStatus.clothesState[Constant.ShoeCategory] == 0;

    protected override void OnCardBeingSaved(GameMode currentGameMode)
    {
    }

    protected override void OnReload(GameMode currentGameMode, bool maintainState)
    {
        SetUpShoes();
    }

    public void SetUpShoes()
    {
        if (ChaControl == null || !IsShoeActive)
            return;

        var shoeID = ChaControl.nowCoordinate.clothes.parts[Constant.ShoeCategory].id;

        Logger.Log($"Looking for ID: \"{shoeID}\"");
        if (Values.Configs.TryGetValue(shoeID, out var shoeConfig) && shoeConfig.loaded)
        {
            Logger.Log("Found! Removing Transforms..");
            RemoveLocalTransforms();
            Logger.Log("Found! Setting up Transforms..");
            LocalTransforms(shoeConfig);
            EnableHover();
        }
        else
        {
            Logger.Log("Not Found! Removing Transforms..");
            RemoveLocalTransforms();
        }
    }

    public void DisableHover()
    {
        if (_hoverSwitch == false)
            return;
        var instanceTransform = ChaControl.gameObject.transform;
        var childTransform = instanceTransform.GetChild(0);
        childTransform.localPosition = Vector3.zero;
        _hoverSwitch = false;
    }

    public void EnableHover()
    {
        // TODO: Unfuck this.
        //if (hoverSwitch)
        //    return;
        var instanceTransform = ChaControl.gameObject.transform;
        var childTransform = instanceTransform.GetChild(0);
        if (_currentConfig == null) return;
        childTransform.localPosition = new Vector3(0, _currentConfig.rootMove.y, 0);
        _hoverSwitch = true;
    }


    public void UpdateHover()
    {
        if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio)
        {
            if (!IsShoeActive || _currentConfig == null || !GroundAnim)
                DisableHover();
            else
                EnableHover();
        }
        else
        {
            if (!IsShoeActive || _currentConfig == null)
                DisableHover();
            else
                EnableHover();
        }
    }

    public void RemoveLocalTransforms()
    {
        if (ChaControl == null)
            return;
        _currentConfig = null;

        DisableHover();
        _transformVectors.Clear();
        _parentDerivation.Clear();

        var ik = ChaControl.fullBodyIK.solver;
        ik.OnPostUpdate = (IKSolver.UpdateDelegate) Delegate.Remove(ik.OnPostUpdate, new IKSolver.UpdateDelegate(IKArray));
    }

    public void LocalTransforms(HeelConfig heelConfig)
    {
        if (ChaControl == null)
            return;
        _currentConfig = heelConfig;

        foreach (var pair in Constant.pathMaps)
        {
            heelConfig.heelVectors.TryGetValue(pair.Value, out var partVectors);

            if (partVectors == null) continue;
            var boneTransform = ChaControl.gameObject?.transform?.Find(pair.Key);

            if (boneTransform != null &&
                partVectors.TryGetValue("roll", out var roll) &&
                partVectors.TryGetValue("move", out var move) &&
                partVectors.TryGetValue("scale", out var scale))
            {
                partVectors.TryGetValue("rollmin", out var rollMin);
                partVectors.TryGetValue("rollmax", out var rollMax);

                _transformVectors.Add(boneTransform, new Vector3[5] {move, roll, scale, rollMin, rollMax});
            }

            if (heelConfig.isFixed.TryGetValue(pair.Value, out var parentDerive))
                _parentDerivation.Add(boneTransform, parentDerive);
        }

        var ik = ChaControl.fullBodyIK.solver;
        ik.OnPostUpdate = (IKSolver.UpdateDelegate) Delegate.Combine(ik.OnPostUpdate, new IKSolver.UpdateDelegate(IKArray));
    }

    public void IKArray()
    {
        if (!IsShoeActive)
            return;

        foreach (var macros in _transformVectors)
        {
            var targetTransform = macros.Key;

            targetTransform.localPosition += macros.Value[0];
            targetTransform.localScale = macros.Value[2];

            if (_parentDerivation.ContainsKey(targetTransform) && _parentDerivation[targetTransform])
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