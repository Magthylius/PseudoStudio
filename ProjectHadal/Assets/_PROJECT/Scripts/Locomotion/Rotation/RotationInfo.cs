using Tenshi;
using Hadal.Inputs;
using UnityEngine;
using NaughtyAttributes;

//Created by Jet, E: Jon
namespace Hadal.Locomotion
{
    [System.Serializable]
    public class RotationInfo
    {
        [Header("Input Settings")]
        public float Sensitivity;
        public float Acceleration;
        public float XAxisClamp;
        public float ZAxisClamp;
        public float ClampTolerance;
        public float PullBackSpeed;
        [ReadOnly] public float CurrentXAxis;

        [Space(10f)]
        public float XAxisInputClamp;
        public float YAxisInputClamp;

        [Header("Z Rotation Stabiliser")]
        public float ZDampingSpeed;
        public float ZActiveRotationSpeed;
        public float SnapAngle;
        [ReadOnly] public float ZClampAngle;
        
        private float _currentDampSpeed = 0.0f;
        private bool _isVerticallyObserving = false;
        private const float TiltAngle = 90.0f;
        private const float FullAngle = 360.0f;

        public void Initialise()
        {
            XAxisClamp = XAxisClamp.Abs();
            CurrentXAxis = 0.0f;
            ZClampAngle = 0.0f;
            SnapAngle = SnapAngle.Abs();
        }

        public void DoRotationWithLerp(in IRotationInput input, in float deltaTime, Transform target)
        {
            Vector3 rotation = target.rotation.eulerAngles;
            float inputY = input.YAxis * Sensitivity, inputX = input.XAxis * Sensitivity;
            float targetX = rotation.x - inputY - float.Epsilon;
            float targetY = rotation.y + inputX - float.Epsilon;
            rotation.x = Mathf.LerpAngle(rotation.x, targetX, Acceleration * deltaTime);
            rotation.y = Mathf.LerpAngle(rotation.y, targetY, Acceleration * deltaTime);
            rotation.z = RotateZAxisWithLerpClamp(input, rotation, deltaTime);
            target.rotation = Quaternion.Euler(rotation);
        }

        public void DoSmoothRotation(in IRotationInput input, in float deltaTime, Transform target)
        {
            Vector2 mouseDistance = new Vector2(input.XAxis, input.YAxis);
            mouseDistance *= Sensitivity * Acceleration * deltaTime;

            Vector3 rotation = target.localRotation.eulerAngles;
            rotation.x = rotation.x.NormalisedAngle();
            rotation.y = rotation.y.NormalisedAngle();
            rotation.z = RotateZAxisWithLerpClamp(input, rotation, deltaTime);
            target.localRotation = Quaternion.Euler(rotation);
            target.Rotate(-mouseDistance.y, mouseDistance.x, 0.0f, Space.Self);
        }

        public void DoLocalRotation(in IRotationInput input, in float deltaTime, Transform target)
        {
            Vector2 mouseDistance = new Vector2(input.XAxis, input.YAxis);
            mouseDistance *= (Sensitivity * Acceleration * deltaTime);

            Vector3 rotation = target.localRotation.eulerAngles;
            rotation.x -= mouseDistance.y;
            rotation.y += mouseDistance.x;

            target.localRotation = Quaternion.Lerp(target.localRotation, Quaternion.Euler(rotation), 5f * deltaTime);

            rotation = target.rotation.eulerAngles;
            rotation.z = Mathf.Clamp(-mouseDistance.x, -ZAxisClamp, ZAxisClamp);
            target.rotation = Quaternion.Lerp(target.rotation, Quaternion.Euler(rotation), 5f * deltaTime);
        }

        private static Vector3 ReverseZAxis(Vector3 rotation)
        {
            Vector3 postRot = rotation;
            if (postRot.z >= 0f && postRot.z < 180f)
            {
                postRot.z = 360f - postRot.z;
            }
            else// if (postRot.z < 360f && postRot.z > 180f)
            {
                postRot.z = 0f + (360f - postRot.z.Abs());
            }
            return postRot;
        }

        private float RotateZAxisWithLerpClamp(in IRotationInput input, in Vector3 euler, in float deltaTime)
        {
            float z = euler.z;
            if (HandleZAxisClamp(euler, deltaTime, z))
            {
                z = Mathf.SmoothDampAngle(z, ZClampAngle, ref _currentDampSpeed, SmoothingTime, MaxDampSpeed, SmoothDampStep * deltaTime);
            }

            return z * (!SnapDistanceReached()).AsFloat() + ZClampAngle * SnapDistanceReached().AsFloat();

            #region Local Shorthands
            bool SnapDistanceReached() => z.NormalisedAngle().DiffFrom(ZClampAngle.NormalisedAngle()).IsLowerThan(SnapAngle);
            #endregion
        }
        
        private bool HandleZAxisClamp(Vector3 rotation, in float deltaTime, float z)
        {
            CurrentXAxis = rotation.x;
            CurrentXAxis = CurrentXAxis.NormalisedAngle();
            float normAxisClamp = FullAngle - XAxisClamp;
            if ((UpperClamped() || LowerClamped()) && !_isVerticallyObserving) _isVerticallyObserving = true;
            if (_isVerticallyObserving && HasReturnedToClamp(TiltAngle - XAxisClamp)) _isVerticallyObserving = false;

            return !_isVerticallyObserving;
            
            #region Local Shorthands
            bool UpperClamped() => CurrentXAxis < normAxisClamp && CurrentXAxis > 180.0f;
            bool LowerClamped() => CurrentXAxis > XAxisClamp && CurrentXAxis < 180.0f;
            bool HasReturnedToClamp(float yAxisDeviation)
            {
                return CurrentXAxis < FullAngle - TiltAngle - ClampTolerance - yAxisDeviation
                    || CurrentXAxis > FullAngle - TiltAngle + ClampTolerance + yAxisDeviation;
            }
            #endregion
        }

        private Vector3 HandleAxisClamp(Vector3 rotationSelf, in float deltaTime)
        {
            CurrentXAxis = rotationSelf.x;
            CurrentXAxis = CurrentXAxis.NormalisedAngle();

            float normAxisClamp = FullAngle - XAxisClamp;
            float upperClamp = normAxisClamp - ClampTolerance;
            float lowerClamp = XAxisClamp + ClampTolerance;
            if (CurrentXAxis < normAxisClamp && CurrentXAxis > 180.0f)
            {
                CurrentXAxis = Mathf.LerpAngle(CurrentXAxis, normAxisClamp, PullBackSpeed * deltaTime);
                if (CurrentXAxis < upperClamp) CurrentXAxis = upperClamp;
                rotationSelf.x = CurrentXAxis;
                return rotationSelf;
            }
            if (CurrentXAxis > XAxisClamp && CurrentXAxis < 180.0f)
            {
                CurrentXAxis = Mathf.LerpAngle(CurrentXAxis, XAxisClamp, PullBackSpeed * deltaTime);
                if (CurrentXAxis > lowerClamp) CurrentXAxis = lowerClamp;
                rotationSelf.x = CurrentXAxis;
            }
            return rotationSelf;
        }

        private float MaxDampSpeed => ZActiveRotationSpeed * Acceleration;
        private float SmoothingTime => MaxDampSpeed * Sensitivity;
        private float SmoothDampStep => ZActiveRotationSpeed * Acceleration * Sensitivity * ZDampingSpeed;
    }
}