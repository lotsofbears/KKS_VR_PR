using System.Linq;
using UnityEngine;
using VRGIN.Controls;
using VRGIN.Core;
using VRGIN.Helpers;
using KKS_VR.Settings;


public enum POV_MODE
{
    // Press Key (default: Y) to change POV Mode, configurable in BepInEX Plugin Settings (F1)
    EYE = 0,      // Mode1: Tracking Eye Position & Rotation
    HEAD = 1,     // Mode2: Only Tracking Eye Position (Default)
    TELEPORT = 2, // Mode3: Teleport(Jump) to next character when trigger controller
    TOTAL = 3     // Total num of Enum
}

namespace KKS_VR.Camera
{
    internal class POV : ProtectedBehaviour
    {
        private Controller _leftController, _rightController;

        private ChaControl _currentChara;

        private POV_MODE _povMode;

        private bool _active;

        private KoikatuSettings _settings;

        private Quaternion _tmp_head_y; // for POV_MODE.EYE

        public void Initialize(Controller left, Controller right)
        {
            _leftController = left;
            _rightController = right;
        }

        public bool IsActive()
        {
            return _active;
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            // HScene Init
            _currentChara = null;

            _povMode = POV_MODE.HEAD;

            _active = false;

            _settings = VR.Context.Settings as KoikatuSettings;

            _tmp_head_y = Quaternion.identity;
        }

        private void SetCameraToCharEye(ChaControl target)
        {
            var position = GetEyes(target).position;
            var rotation = GetEyes(target).rotation;

            // Set Camera to Character Eye
            VRManager.Instance.Mode.MoveToPosition(position, rotation, false);
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
            if (!transform)
            {
                VRLog.Info("Creating eyes, {0}", human.name);
                transform = new GameObject("cf_j_faceup_tz").transform;
                transform.SetParent(GetHead(human), false);
                transform.transform.localPosition = new Vector3(0f, 0.07f, 0.05f);
            }
            else
            {
                VRLog.Debug("Found eyes, {0}", human.name);
            }
            return transform;
        }
        private int getCurrChaIdx(ChaControl[] targets)
        {
            if (_currentChara)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (ChaControl.ReferenceEquals(targets[i], _currentChara) &&
                       ((_currentChara.sex == (int)POVConfig.targetGender.Value) || (POVConfig.targetGender.Value == POVConfig.Gender.All)))
                    {
                        return i;
                    }
                }
            }
            // cannot find last pov character (initialize / deleted)
            return targets.Length - 1;
        }

        // set _currentChara
        private void nextChar(ChaControl[] targets, int currentTargetIndex, bool keep_char = false)
        {
            var cnt = 0;

            while (cnt < targets.Length)
            {
                if ((keep_char && cnt == 0) == false)
                {
                    currentTargetIndex = (currentTargetIndex + 1) % targets.Length;
                }
                _currentChara = targets[currentTargetIndex];

                if (_currentChara)
                {
                    if ((POVConfig.targetGender.Value == POVConfig.Gender.All) || (_currentChara.sex == (int)POVConfig.targetGender.Value)) // 1 is female
                    {
                        if (_povMode == POV_MODE.EYE || _povMode == POV_MODE.HEAD || _povMode == POV_MODE.TELEPORT)
                        {
                            SetCameraToCharEye(_currentChara);

                            if (_povMode == POV_MODE.EYE)
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
            _currentChara = null;
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

        private void SetPOV()
        {
            if (!_active) return;

            if (_currentChara)
            {
                switch (_povMode)
                {
                    case POV_MODE.EYE:
                        MoveToPositionEx(GetEyes(_currentChara).position, GetEyes(_currentChara).rotation);
                        break;

                    case POV_MODE.HEAD:
                        VRManager.Instance.Mode.MoveToPosition(GetEyes(_currentChara).position, false);
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
            if (POVConfig.switchPOVModeKey.Value == POVConfig.POVKeyList.VR_TRIGGER || POVConfig.switchPOVModeKey.Value == POVConfig.POVKeyList.VR_BUTTON2) // Use VR button
            {
                var button_id = (POVConfig.switchPOVModeKey.Value == POVConfig.POVKeyList.VR_TRIGGER) ? Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger : Valve.VR.EVRButtonId.k_EButton_A;
                if (_leftController.Input.GetPressDown(button_id) || _rightController.Input.GetPressDown(button_id))
                {
                    if (_povMode == POV_MODE.EYE) { resetRotationXZ(); }
                    _povMode = (POV_MODE)(((int)(_povMode + 1)) % (int)(POV_MODE.TOTAL));
                }
            }
            else if (Input.GetKeyDown((KeyCode)POVConfig.switchPOVModeKey.Value)) // Use Keyboard Key
            {
                if (_povMode == POV_MODE.EYE) { resetRotationXZ(); }
                _povMode = (POV_MODE)(((int)(_povMode + 1)) % (int)(POV_MODE.TOTAL));
            }

            if (!_active) // Activate POV under Hand Tool if user press VR Trigger button / Key 
            {
                if (POVConfig.POVKey.Value == POVConfig.POVKeyList.VR_TRIGGER || POVConfig.POVKey.Value == POVConfig.POVKeyList.VR_BUTTON2) // Use VR button
                {
                    var button_id = (POVConfig.POVKey.Value == POVConfig.POVKeyList.VR_TRIGGER) ? Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger : Valve.VR.EVRButtonId.k_EButton_A;
                    // When left hand controller is Hand Tool and visible, Press VR button to set _active
                    if (VR.Mode.Left.ToolIndex == 2 && VR.Mode.Left.ActiveTool.isActiveAndEnabled && _leftController.Input.GetPressDown(button_id))
                    {
                        _active = true;
                        initPOVTarget();
                        return;
                    }
                    // When right hand controller is Hand Tool and visible, Press VR button to set _active
                    else if (VR.Mode.Right.ToolIndex == 2 && VR.Mode.Right.ActiveTool.isActiveAndEnabled && _rightController.Input.GetPressDown(button_id))
                    {
                        _active = true;
                        initPOVTarget();
                        return;
                    }
                }
                else // Use Keyboard Key
                {
                    if ((VR.Mode.Left.ToolIndex == 2 || VR.Mode.Right.ToolIndex == 2) && Input.GetKeyDown((KeyCode)POVConfig.POVKey.Value))
                    {
                        _active = true;
                        initPOVTarget();
                        return;
                    }
                }
            }
            // When there is no Hand Tool, deactive
            else if (_active && VR.Mode.Left.ToolIndex != 2 && VR.Mode.Right.ToolIndex != 2)
            {
                _active = false;
                if (_povMode == POV_MODE.EYE) { resetRotationXZ(); }
                return;
            }

            if (_active)
            {
                if (POVConfig.POVKey.Value == POVConfig.POVKeyList.VR_TRIGGER || POVConfig.POVKey.Value == POVConfig.POVKeyList.VR_BUTTON2) // Use VR button
                {
                    var button_id = (POVConfig.POVKey.Value == POVConfig.POVKeyList.VR_TRIGGER) ? Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger : Valve.VR.EVRButtonId.k_EButton_A;
                    // Press VR button to change target POV character
                    if (VR.Mode.Left.ToolIndex == 2 && VR.Mode.Left.ActiveTool.isActiveAndEnabled && _leftController.Input.GetPressDown(button_id))
                    {
                        SwitchToNextChar();
                    }
                    else if (VR.Mode.Right.ToolIndex == 2 && VR.Mode.Right.ActiveTool.isActiveAndEnabled && _rightController.Input.GetPressDown(button_id))
                    {
                        SwitchToNextChar();
                    }
                }
                else // Use Keyboard Key
                {
                    if (Input.GetKeyDown((KeyCode)POVConfig.POVKey.Value)) { SwitchToNextChar(); }
                }
            }


            SetPOV();
        }
    }
}
