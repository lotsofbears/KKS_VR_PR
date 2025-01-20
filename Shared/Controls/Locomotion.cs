using System;
using System.Collections.Generic;
using System.Text;
using KK_VR.Handlers;
using KK_VR.Settings;
using UnityEngine;
using VRGIN.Controls;
using VRGIN.Core;

namespace KK_VR.Controls
{
    // Can be rough after awhile, as the locomotion is very smooth. 
    // Kinda? the same idea like in Blade and Sorcery game, move and/or rotate in the direction of thumbstick.
    internal class Locomotion
    {
        internal Locomotion(Controller controller)
        {
            _controller = controller;
        }
        private readonly Controller _controller;
        private float _current;
        private float _currentDelta;
        private float _currentVelocity;
        private Quaternion _currentRot;
        private Vector2 _nonZeroVec;
        private float _distance;
        internal Vector2 GetAxis
        {
            get
            {
                var axis = _controller.Input.GetAxis();
                _distance = axis.magnitude;

                // Keep old non zero axis to deaccelerate smoothly after ending;
                if (_distance != 0f)
                {
                    _nonZeroVec = axis;
                }
                return _nonZeroVec;
            }
        }
        private Vector3 GetVec => _currentDelta * (_currentRot * Vector3.forward);
        private Quaternion GetRot
        {
            get
            {
                var originRot = VR.Camera.Origin.rotation;
                var targetRot = originRot * (Quaternion.Inverse(originRot) * _currentRot);
                var angle = Quaternion.Angle(originRot, targetRot);

                return angle > 5f ? Quaternion.RotateTowards(originRot, targetRot, _currentDelta * angle) : VR.Camera.Origin.rotation;
            }
        }

        private bool Update()
        {
            var axis = GetAxis;
            var target = _distance < 0.2f ? 0f : 1f;
            var smoothTime = target == 0f ? 0.25f : 1f;

            _current = Mathf.SmoothDamp(_current, target, ref _currentVelocity, smoothTime);

            if (_current > 0.001f)
            {
                var angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg;

                // No clue why flip is required.
                angle = 0f - (angle - 90f);

                _currentDelta = _current * Time.deltaTime;
                _currentRot = Quaternion.Euler(0f, angle + _controller.transform.rotation.eulerAngles.y, 0f);

                return true;
            }
            return false;
        }
        internal bool MoveRotate(Vector3 deltaPos)
        {
            if (Update())
            {
                var origin = VR.Camera.Origin;
                origin.SetPositionAndRotation(origin.position + deltaPos + GetVec, GetRot);
                return true;
            }
            return false;
        }
        internal bool Move(Vector3 deltaPos)
        {
            if (Update())
            {
                VR.Camera.Origin.position += deltaPos + GetVec;
                return true;
            }
            return false;
        }
    }
}
