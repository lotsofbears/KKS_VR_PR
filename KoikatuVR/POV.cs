using KoikatuVR.Caress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VRGIN.Core;
using VRGIN.Helpers;


public enum POV_MODE
{
    // Press Key (default: Y) to change POV Mode, configurable in BepInEX Plugin Settings (F1)
    EYE = 0,      // Mode1: Tracking Eye Position & Rotation
    HEAD = 1,     // Mode2: Only Tracking Eye Position (Default)
    TELEPORT = 2, // Mode3: Teleport(Jump) to next character when trigger controller
    TOTAL = 3     // Total num of Enum
}

namespace KoikatuVR
{
    public class POV : ProtectedBehaviour
    {

        private ChaControl _currentTarget;

        private POV_MODE _povMode;

        private bool _active;

        private KoikatuSettings _settings;

        private Quaternion _tmp_head_y; // for POV_MODE.EYE

        protected override void OnAwake()
        {
            base.OnAwake();
            // HScene Init
            _currentTarget = null;

            _povMode = POV_MODE.HEAD;

            _active = false;

            _settings = VR.Context.Settings as KoikatuSettings;

            _tmp_head_y = Quaternion.identity;
        }

        private void SetCameraToCharEye(ChaControl target)
        {
            var _position = GetEyes(target).position;
            var _rotation = GetEyes(target).rotation;

            // Set Camera to Character Eye
            VRManager.Instance.Mode.MoveToPosition(_position, _rotation, false);
        }

        // from VRGIN ControlMode.cs
        private void MoveToPositionEx(Vector3 targetPosition, Quaternion rotation = default(Quaternion))
        {
            // Camera rotates with character eye
            VR.Camera.SteamCam.origin.rotation = rotation * _tmp_head_y;

            float targetY = targetPosition.y;
            float myY = VR.Camera.SteamCam.head.position.y;
            targetPosition = new Vector3(targetPosition.x, targetY, targetPosition.z);
            var myPosition = new Vector3(VR.Camera.SteamCam.head.position.x, myY, VR.Camera.SteamCam.head.position.z);
            VR.Camera.SteamCam.origin.position += (targetPosition - myPosition);
        }

        // from VRGIN ControlMode.cs
        private Quaternion MakeUpright(Quaternion rotation)
        {
            return Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        }
        private void resetRotationXZ()
        {
            VR.Camera.SteamCam.origin.rotation = MakeUpright(VR.Camera.SteamCam.origin.rotation);
            _tmp_head_y = Quaternion.identity;
        }

        private Transform GetHead(ChaControl human)
        {
            return human.objHead.GetComponentsInParent<Transform>().First((Transform t) => t.name.StartsWith("c") && t.name.ToLower().Contains("j_head"));
        }
        private Transform GetEyes(ChaControl human)
        {
            Transform transform = human.objHeadBone.transform.Descendants().FirstOrDefault((Transform t) => t.name.StartsWith("c") && t.name.ToLower().EndsWith("j_faceup_tz"));
            if(!transform)
            {
                VRLog.Debug("Creating eyes", new object[0]);
                transform = new GameObject("cf_j_faceup_tz").transform;
                transform.SetParent(GetHead(human), false);
                transform.transform.localPosition = new Vector3(0f, 0.07f, 0.05f);
            }
            else
            {
                VRLog.Debug("found eyes", new object[0]);
            }
            return transform;
        }
        private int getCurrChaIdx(ChaControl[] targets)
        {
            if(_currentTarget)
            {
                for(int i = 0; i < targets.Length; i++)
                {
                    if(ChaControl.ReferenceEquals(targets[i], _currentTarget) && _currentTarget.sex != 1)
                    {
                        return i;
                    }
                }
            }
            // cannot find last pov character (initialize / deleted)
            return targets.Length - 1;
        }

        // set _currentTarget
        private void nextChar(ChaControl[] targets, int currentTargetIndex, bool keep_char = false)
        {
            var cnt = 0;

            while (cnt < targets.Length)
            {
                if((keep_char && cnt == 0) == false)
                {
                    currentTargetIndex = (currentTargetIndex + 1) % targets.Length;
                }
                _currentTarget = targets[currentTargetIndex];

                if (_currentTarget)
                {
                    if (_currentTarget.sex != 1) // 1 is female, only choose male as target
                    {
                        if (_povMode == POV_MODE.EYE || _povMode == POV_MODE.HEAD || _povMode == POV_MODE.TELEPORT)
                        {
                            SetCameraToCharEye(_currentTarget);

                            if(_povMode == POV_MODE.EYE)
                            {
                                // _tmp_head_y : the initial y when jump to character
                                _tmp_head_y = MakeUpright(VR.Camera.SteamCam.origin.rotation) * 
                                                               Quaternion.Inverse(MakeUpright(VR.Camera.SteamCam.head.rotation));
                            }
                        }
                        return;
                    }
                }
                cnt++;
            }
            // target character not found
            _currentTarget = null;
        }

        public void SwitchToNextChar()
        {
            // Change to next target character
            if (!_active) return;

            ChaControl[] targets = GameObject.FindObjectsOfType<ChaControl>();
            var currentTargetIndex = getCurrChaIdx(targets);
            nextChar(targets, currentTargetIndex, false);
        }

        private void initPOVTarget()
        {
            // Inherit last target character if still alive, otherwise find a new target character
            ChaControl[] targets = GameObject.FindObjectsOfType<ChaControl>();
            var currentTargetIndex = getCurrChaIdx(targets);
            nextChar(targets, currentTargetIndex, true);
        }

        private void setPOV()
        {
            if (!_active) return;

            if (_currentTarget)
            {
                switch (_povMode)
                {
                    case POV_MODE.EYE:
                        MoveToPositionEx(GetEyes(_currentTarget).position, GetEyes(_currentTarget).rotation);
                        break;

                    case POV_MODE.HEAD:
                        VRManager.Instance.Mode.MoveToPosition(GetEyes(_currentTarget).position, false);
                        break;

                    case POV_MODE.TELEPORT:
                        // Already Teleport in SwitchToNextChar
                        break;

                    default:
                        break;
                }
            }   
        }

        protected override void OnUpdate()
        {
            if (_settings.EnablePOV == false) return;
            // Press Key (default: Y) to change POV Mode, configurable in BepInEX Plugin Settings (F1)
            if (POVConfig.switchPOVModeKey.Value.IsDown())
            {
                if (_povMode == POV_MODE.EYE) {resetRotationXZ();}
                _povMode = (POV_MODE)(((int)(_povMode + 1)) % (int)(POV_MODE.TOTAL));
            }

            // When left hand controller is Hand Tool and visible, Press VR Trigger button to set _active
            if ((!_active) && VR.Mode.Left.ToolIndex == 2 && VR.Mode.Left.ActiveTool.isActiveAndEnabled)
            {
                var device = SteamVR_Controller.Input((int)VR.Mode.Left.Tracking.index);
                if (device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
                {
                    _active = true;
                    initPOVTarget();
                    return;
                }
            }
            // When right hand controller is Hand Tool and visible, Press VR Trigger button to set _active
            else if ((!_active) && VR.Mode.Right.ToolIndex == 2 && VR.Mode.Right.ActiveTool.isActiveAndEnabled)
            {
                var device = SteamVR_Controller.Input((int)VR.Mode.Right.Tracking.index);
                if (device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
                {
                    _active = true;
                    initPOVTarget();
                    return;
                }
            }
            // When there is no Hand Tool, deactive
            else if (_active && 
                     (VR.Mode.Left.ToolIndex != 2 && VR.Mode.Right.ToolIndex != 2))
            {
                _active = false;
                if (_povMode == POV_MODE.EYE) {resetRotationXZ();}
                return;
            }

            if (_active)
            {
                // Press VR Trigger button to change target POV character
                if (VR.Mode.Left.ToolIndex == 2 && VR.Mode.Left.ActiveTool.isActiveAndEnabled)
                {
                    var device = SteamVR_Controller.Input((int)VR.Mode.Left.Tracking.index);
                    if (device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {SwitchToNextChar();}
                }
                else if (VR.Mode.Right.ToolIndex == 2 && VR.Mode.Right.ActiveTool.isActiveAndEnabled)
                {
                    var device = SteamVR_Controller.Input((int)VR.Mode.Right.Tracking.index);
                    if (device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {SwitchToNextChar();}
                }
            }


            setPOV();
        }
    }
}
