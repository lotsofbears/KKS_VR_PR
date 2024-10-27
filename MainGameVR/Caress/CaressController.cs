using System;
using System.Collections.Generic;
using HarmonyLib;
using KKS_VR.Settings;
using Manager;
using UnityEngine;
using Valve.VR;
using VRGIN.Controls;
using VRGIN.Core;
using VRGIN.Helpers;

namespace KKS_VR.Caress
{
    /// <summary>
    /// An extra component to be attached to each controller, providing the caress
    /// functionality in H scenes.
    /// This component is designed to exist only for the duration of an H scene.
    /// </summary>
    internal class CaressController : ProtectedBehaviour
    {
        // Basic plan:
        //
        // * Keep track of the potential caress points
        //   near this controller. _aibuTracker is responsible for this.
        // * While there is at least one such point, lock the controller
        //   to steal any trigger events.
        // * When the trigger is pulled, initiate caress.
        // * Delay releasing of the lock until the trigger is released.

        KoikatuSettings _settings;
        Controller _controller;
        AibuColliderTracker _aibuTracker;
        Undresser _undresser;
        Controller.Lock _lock; // may be null but never invalid
        bool _triggerPressed; // Whether the trigger is currently pressed. false if _lock is null.
        ValueTuple<ChaControl, ChaFileDefine.ClothesKind, Vector3>? _undressing;

        public Controller getController()
        {
            return _controller;
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            _settings = VR.Context.Settings as KoikatuSettings;
            _controller = GetComponent<Controller>();
            var proc = FindObjectOfType<HSceneProc>();
            if (proc == null)
            {
                VRLog.Warn("HSceneProc not found");
                return;
            }
            _aibuTracker = new AibuColliderTracker(proc, referencePoint: transform);
            _undresser = new Undresser(proc);
        }

        private void OnDestroy()
        {
            if (_lock != null) ReleaseLock();
        }

        protected override void OnUpdate()
        {
            if (_lock != null)
            {
                HandleTrigger();
                HandleToolChange();
                HandleUndress();
            }
            UpdateLock();
        }

        protected void OnTriggerEnter(Collider other)
        {
            try
            {
                if (Scene.NowSceneNames[0] == "HPointMove") return;
                var wasIntersecting = _aibuTracker.IsIntersecting();
                if (_aibuTracker.AddIfRelevant(other))
                {
                    if (!wasIntersecting && _aibuTracker.IsIntersecting())
                    {
                        _controller.StartRumble(new RumbleImpulse(1000));
                        if (_settings.AutomaticTouching)
                        {
                            var colliderKind = _aibuTracker.GetCurrentColliderKind(out int femaleIndex);
                            if (HandCtrl.AibuColliderKind.reac_head <= colliderKind &&
                                !CaressUtil.IsSpeaking(_aibuTracker.Proc, femaleIndex))
                            {
                                CaressUtil.SetSelectKindTouch(_aibuTracker.Proc, femaleIndex, colliderKind);
                                StartCoroutine(CaressUtil.ClickCo());
                            }
                        }
                    }
                }

                _undresser.Enter(other);
                UpdateLock();
            }
            catch (Exception e)
            {
                VRLog.Error(e);
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            try
            {
                _aibuTracker.RemoveIfRelevant(other);
                _undresser.Exit(other);
                UpdateLock();
            }
            catch (Exception e)
            {
                VRLog.Error(e);
            }
        }

        private void UpdateLock()
        {
            bool shouldHaveLock = (_aibuTracker.IsIntersecting() || _undressing != null) &&
                    Manager.Scene.NowSceneNames[0] != "HPointMove";
            if (shouldHaveLock && _lock == null)
                _controller.TryAcquireFocus(out _lock);
            else if (!shouldHaveLock && _lock != null && !_triggerPressed) ReleaseLock();
        }

        private void HandleTrigger()
        {
            var device = _controller.Input; //SteamVR_Controller.Input((int)_controller.Tracking.index);
            if (!_triggerPressed && device.GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger))
            {
                UpdateSelectKindTouch();
                HandCtrlHooks.InjectMouseButtonDown(0);
                _controller.StartRumble(new RumbleImpulse(1000));
                _triggerPressed = true;
            }
            else if (_triggerPressed && device.GetPressUp(EVRButtonId.k_EButton_SteamVR_Trigger))
            {
                HandCtrlHooks.InjectMouseButtonUp(0);
                _triggerPressed = false;
                UpdateLock();
            }
        }

        private void HandleToolChange()
        {
            var device = _controller.Input; //SteamVR_Controller.Input((int)_controller.Tracking.index);
            if (device.GetPressUp(EVRButtonId.k_EButton_ApplicationMenu))
            {
                UpdateSelectKindTouch();
                HandCtrlHooks.InjectMouseScroll(1f);
            }

        }

        private void HandleUndress()
        {
            var device =  _controller.Input;
            var proc = _aibuTracker.Proc;
            if (_undressing == null && device.GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad))
            {
                var females = new Traverse(proc).Field<List<ChaControl>>("lstFemale").Value;
                var toUndress = _undresser.ComputeUndressTarget(females, out int femaleIndex);
                if (toUndress is ChaFileDefine.ClothesKind kind)
                {
                    _undressing = ValueTuple.Create(females[femaleIndex], kind, transform.position);
                }
            }
            if (_undressing is ValueTuple<ChaControl, ChaFileDefine.ClothesKind, Vector3> undressing
                && device.GetPressUp(EVRButtonId.k_EButton_SteamVR_Touchpad))
            {
                if (0.3f * 0.3f < (transform.position - undressing.Item3).sqrMagnitude)
                {
                    undressing.Item1.SetClothesState((int)undressing.Item2, 3);
                }
                else
                {
                    undressing.Item1.SetClothesStateNext((int)undressing.Item2);
                }
                _undressing = null;
            }
        }

        private void ReleaseLock()
        {
            CaressUtil.SetSelectKindTouch(_aibuTracker.Proc, 0, HandCtrl.AibuColliderKind.none);
            if (_triggerPressed)
                HandCtrlHooks.InjectMouseButtonUp(0);
            _triggerPressed = false;
            _undressing = null;
            _lock.Release();
            _lock = null;
        }

        private void UpdateSelectKindTouch()
        {
            var colliderKind = _aibuTracker.GetCurrentColliderKind(out var femaleIndex);
            CaressUtil.SetSelectKindTouch(_aibuTracker.Proc, femaleIndex, colliderKind);
        }
    }
}
