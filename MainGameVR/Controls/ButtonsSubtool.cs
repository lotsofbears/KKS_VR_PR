using System;
using System.Collections.Generic;
using System.Linq;
using KKS_VR.Camera;
using KKS_VR.Interpreters;
using KKS_VR.Settings;
using UnityEngine;
using VRGIN.Core;
using WindowsInput.Native;

namespace KKS_VR.Controls
{
    /// <summary>
    /// A subtool that handles an arbitrary number of simple actions that only
    /// requires a single button.
    /// </summary>
    internal class ButtonsSubtool
    {
        private readonly KoikatuInterpreter _Interpreter;

        /// <summary>
        /// The set of keys for which we've sent a down message but not a
        /// corresponding up message.
        /// </summary>
        private readonly HashSet<AssignableFunction> _SentUnmatchedDown
            = new HashSet<AssignableFunction>();

        private readonly KoikatuSettings _Settings;
        private int _ScrollRepeatAmount;

        private float _ScrollRepeatTime;

        public ButtonsSubtool(KoikatuInterpreter interpreter, KoikatuSettings settings)
        {
            _Interpreter = interpreter;
            _Settings = settings;
        }

        /// <summary>
        /// A method to be called in Update().
        /// </summary>
        public void Update()
        {
            if (_SentUnmatchedDown.Contains(AssignableFunction.PL2CAM)) IfActionScene(interpreter => interpreter.MovePlayerToCamera());
            if (_ScrollRepeatAmount != 0 && _ScrollRepeatTime < Time.unscaledTime)
            {
                _ScrollRepeatTime += 0.1f;
                VR.Input.Mouse.VerticalScroll(_ScrollRepeatAmount);
            }
        }

        /// <summary>
        /// Whether it's desirable to lock the controller.
        /// </summary>
        /// <returns></returns>
        public bool WantLock()
        {
            return _SentUnmatchedDown.Count > 0;
        }

        /// <summary>
        /// A method to be called when this subtool is destroyed.
        /// </summary>
        public void Destroy()
        {
            // Make a copy because the loop below will modify the HashSet.
            var todo = _SentUnmatchedDown.ToList();
            foreach (var key in todo) ButtonUp(key);
        }

        /// <summary>
        /// Process a ButtonDown message.
        /// </summary>
        public void ButtonDown(AssignableFunction fun)
        {
            switch (fun)
            {
                case AssignableFunction.NONE:
                    break;
                case AssignableFunction.WALK:
                    IfActionScene(interpreter => interpreter.StartWalking());
                    break;
                case AssignableFunction.DASH:
                    IfActionScene(interpreter => interpreter.StartWalking(true));
                    break;
                case AssignableFunction.PL2CAM:
                    break;
                case AssignableFunction.LBUTTON:
                    VR.Input.Mouse.LeftButtonDown();
                    break;
                case AssignableFunction.RBUTTON:
                    VR.Input.Mouse.RightButtonDown();
                    break;
                case AssignableFunction.MBUTTON:
                    VR.Input.Mouse.MiddleButtonDown();
                    break;
                case AssignableFunction.LROTATION:
                    Rotate(-_Settings.RotationAngle);
                    break;
                case AssignableFunction.RROTATION:
                    Rotate(_Settings.RotationAngle);
                    break;
                case AssignableFunction.SCROLLUP:
                    StartScroll(1);
                    break;
                case AssignableFunction.SCROLLDOWN:
                    StartScroll(-1);
                    break;
                case AssignableFunction.CROUCH:
                    IfActionScene(interpreter => interpreter.Crouch());
                    break;
                case AssignableFunction.NEXT:
                    throw new NotSupportedException();
                case AssignableFunction.KEYBOARD_PAGE_DOWN:
                    VR.Input.Keyboard.KeyDown(VirtualKeyCode.NEXT);
                    break;
                default:
                    VR.Input.Keyboard.KeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), fun.ToString()));
                    break;
            }

            _SentUnmatchedDown.Add(fun);
        }

        /// <summary>
        /// Process a ButtonUp message.
        /// </summary>
        public void ButtonUp(AssignableFunction fun)
        {
            switch (fun)
            {
                case AssignableFunction.NONE:
                    break;
                case AssignableFunction.WALK:
                    IfActionScene(interpreter => interpreter.StopWalking());
                    break;
                case AssignableFunction.DASH:
                    IfActionScene(interpreter => interpreter.StopWalking());
                    break;
                case AssignableFunction.PL2CAM:
                    break;
                case AssignableFunction.LBUTTON:
                    VR.Input.Mouse.LeftButtonUp();
                    break;
                case AssignableFunction.RBUTTON:
                    VR.Input.Mouse.RightButtonUp();
                    break;
                case AssignableFunction.MBUTTON:
                    VR.Input.Mouse.MiddleButtonUp();
                    break;
                case AssignableFunction.LROTATION:
                case AssignableFunction.RROTATION:
                    break;
                case AssignableFunction.SCROLLUP:
                case AssignableFunction.SCROLLDOWN:
                    _ScrollRepeatAmount = 0;
                    break;
                case AssignableFunction.CROUCH:
                    IfActionScene(interpreter => interpreter.StandUp());
                    break;
                case AssignableFunction.NEXT:
                    throw new NotSupportedException();
                case AssignableFunction.KEYBOARD_PAGE_DOWN:
                    VR.Input.Keyboard.KeyUp(VirtualKeyCode.NEXT);
                    break;
                default:
                    VR.Input.Keyboard.KeyUp((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), fun.ToString()));
                    break;
            }

            _SentUnmatchedDown.Remove(fun);
        }

        private void StartScroll(int amount)
        {
            VR.Input.Mouse.VerticalScroll(amount);
            _ScrollRepeatTime = Time.unscaledTime + 0.5f;
            _ScrollRepeatAmount = amount;
        }

        /// <summary>
        /// Rotate the camera. If we are in Roaming, rotate the protagonist as well.
        /// </summary>
        private void Rotate(float degrees)
        {
            VRLog.Debug("Rotating {0} degrees", degrees);
            var actInterpreter = _Interpreter.SceneInterpreter as ActionSceneInterpreter;
            if (actInterpreter != null) actInterpreter.MoveCameraToPlayer(true);
            var camera = VR.Camera.transform;
            var newRotation = Quaternion.AngleAxis(degrees, Vector3.up) * camera.rotation;
            VRCameraMover.Instance.MoveTo(camera.position, newRotation, false);
            if (actInterpreter != null) actInterpreter.MovePlayerToCamera();
        }

        private void IfActionScene(Action<ActionSceneInterpreter> a)
        {
            if (_Interpreter.SceneInterpreter is ActionSceneInterpreter actInterpreter) a(actInterpreter);
        }
    }
}
