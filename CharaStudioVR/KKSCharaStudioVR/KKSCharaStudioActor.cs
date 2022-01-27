using System;
using UnityEngine;
using VRGIN.Core;
using VRGIN.Helpers;

namespace KKSCharaStudioVR
{
    public class KKSCharaStudioActor : DefaultActorBehaviour<ChaControl>
    {
        private LookTargetController _TargetController;

        public TransientHead Head { get; private set; }

        public override Transform Eyes => Head.Eyes;

        public override bool HasHead
        {
            get => Head.Visible;
            set => Head.Visible = value;
        }

        public bool IsFemale => Actor.sex == 1;

        protected override void Initialize(ChaControl actor)
        {
            base.Initialize(actor);
            Head = actor.gameObject.AddComponent<TransientHead>();
        }

        protected override void OnStart()
        {
            base.OnStart();
            _TargetController = LookTargetController.AttachTo(this, gameObject);
        }

        private void InitializeDynamicBoneColliders()
        {
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            var eyeLookCtrl = Actor.eyeLookCtrl;
            var neckLookCtrl = Actor.neckLookCtrl;
            var transform = Camera.main.transform;
            if ((bool)transform)
            {
                if ((bool)eyeLookCtrl && eyeLookCtrl.target == transform) eyeLookCtrl.target = _TargetController.Target;
                if ((bool)neckLookCtrl && neckLookCtrl.target == transform) neckLookCtrl.target = _TargetController.Target;
            }

            if (!(Actor.asVoice != null)) return;
            try
            {
                var asVoice = Actor.asVoice;
                asVoice.gameObject.transform.position = Actor.objHeadBone.transform.position;
                var minVoiceDistance = (VR.Settings as KKSCharaStudioVRSettings).MinVoiceDistance;
                var maxVoiceDistance = (VR.Settings as KKSCharaStudioVRSettings).MaxVoiceDistance;
                if (asVoice.minDistance != minVoiceDistance || asVoice.maxDistance != maxVoiceDistance)
                {
                    VRLog.Debug(
                        $"Modify audio parameter {asVoice.name}: ({asVoice.minDistance}, {asVoice.maxDistance}, {asVoice.rolloffMode}) -> ({minVoiceDistance}, {maxVoiceDistance}, {AudioRolloffMode.Logarithmic})");
                    asVoice.minDistance = minVoiceDistance;
                    asVoice.maxDistance = maxVoiceDistance;
                    asVoice.rolloffMode = AudioRolloffMode.Logarithmic;
                }
            }
            catch (Exception obj)
            {
                VRLog.Error(obj);
            }
        }

        internal void OnVRModeChanged(bool newMode)
        {
            if (!(_TargetController != null) || newMode) return;
            var eyeLookCtrl = Actor.eyeLookCtrl;
            var neckLookCtrl = Actor.neckLookCtrl;
            var transform = Camera.main.transform;
            if ((bool)transform)
            {
                if ((bool)eyeLookCtrl && eyeLookCtrl.target == _TargetController.Target) eyeLookCtrl.target = transform;
                if ((bool)neckLookCtrl && neckLookCtrl.target == _TargetController.Target) neckLookCtrl.target = transform;
            }
        }
    }
}