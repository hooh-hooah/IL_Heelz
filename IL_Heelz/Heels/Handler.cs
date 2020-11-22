using System;
using AIChara;
using Heels.Struct;
using RootMotion.FinalIK;
using UnityEngine;

namespace Heels.Handler
{
    public class HeelsHandler
    {
        public bool delegateRegistered;
        public bool IsActive;

        public bool IsHover = true;

        public HeelsHandler(ChaControl chaControl)
        {
            ChaControl = chaControl;
            GameObject = chaControl.gameObject;
            Transform = GameObject.transform;
            ChildTransform = Transform.GetChild(0);
            TargetTransforms = new TransformData(Transform);
            IKSolver = ChaControl.fullBodyIK.solver;
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
            // change the hover behavior based on the game state.
            // calculate if this heel can hover 
            Hover(CanUpdate); // + if it's on some sitting animation state in hscene?
            // spine leg dot prodect to calculate?... bad idea.
        }

        public void UpdateFootAngle()
        {
            if (!CanUpdate) return;
            TargetTransforms.ApplyTransform(Config, ChaControl.animBody.isActiveAndEnabled);
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

        public void Hover(bool hover)
        {
            IsHover = hover;
            ChildTransform.localPosition = !IsHover ? Vector3.zero : new Vector3(0, Config.Root.y, 0);
        }

        public void Reset()
        {
            if (ChaControl == null) return;
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