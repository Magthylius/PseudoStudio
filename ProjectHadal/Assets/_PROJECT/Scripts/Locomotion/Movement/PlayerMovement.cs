﻿using Hadal.Inputs;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;

//Created by Jet
namespace Hadal.Locomotion
{
    [System.Serializable]
    public class PlayerMovement : Mover
    {
        [Header("Debug"), SerializeField] private string debugKey;
        private Vector3 _lastPosition;
        private Vector3 _currentPosition;
        private bool _isLocal = true;

        public override void Initialise(Transform target)
        {
            base.Enable();
            Enable();
            EnableBoost();
            this.target = target;
            Speed.Initialise();
            Accel.Initialise();
            Velocity.Initialise();
            Input = DefaultInputs;
            _lastPosition = target.position;
            _currentPosition = target.position;
            DoDebugEnabling(debugKey);
        }

        public override void DoUpdate(in float deltaTime)
        {
            if (!allowUpdate) return;
            LerpCummulatedAcceleration(deltaTime);
            HandleAcceleration(deltaTime);
            AddVelocity();
            LoseVelocity(deltaTime);
            Move(deltaTime);
            CalculateSpeed(deltaTime);
        }

        public override void DoFixedUpdate(in float fixedDeltaTime)
        {
            
        }

        public override void DoLateUpdate(in float deltaTime)
        {
            
        }

        public void SetIsLocal(bool state) => _isLocal = state;

        static readonly IMovementInput DefaultInputs = new RawKeyboardInput();
        static readonly IMovementInput DisabledInputs = new EmptyKeyboardInput();

        public override void Enable()
        {
            $"Enable is called".Warn();
            Input = DefaultInputs;
        }

        public override void Disable()
        {
            $"Disable is called".Warn();
            Input = DisabledInputs;
        }

        #region Private Methods

        private void HandleAcceleration(in float deltaTime)
        {
            _currentForwardSpeed = VerticalInputSpeed * BoostInputSpeed * Accel.Forward * Accel.CummulatedAcceleration * deltaTime;
            _currentStrafeSpeed = HorizontalInputSpeed * BoostInputSpeed * Accel.Strafe * Accel.CummulatedAcceleration * deltaTime;
            _currentHoverSpeed = HoverInputSpeed * BoostInputSpeed * Accel.Hover * Accel.CummulatedAcceleration * deltaTime;
        }

        private void AddVelocity()
        {
            Velocity.Total += ForwardVelocity + StrafeVelocity + HoverVelocity;
            Velocity.Speed = Velocity.Total.magnitude;
            Velocity.SquareSpeed = Velocity.Speed * Velocity.Speed;
            if (Velocity.Speed > Speed.Max)
            {
                Velocity.Total = Velocity.Total.normalized * Speed.Max;
                Velocity.Speed = Speed.Max;
            }
        }
        private void LoseVelocity(float deltaTime)
        {
            Vector3 total = Velocity.Total;
            Vector3 direction = total.normalized;
            float accelPercent = 10.0f * Mathf.Log(Velocity.SquareSpeed + 1.0f);
            float dragPercent = accelPercent * deltaTime;
            if (dragPercent > Velocity.Speed)
            {
                float averageSpeed = (Speed.InputForward + Speed.InputStrafe + Speed.InputHover) / 3.0f;
                dragPercent = averageSpeed * deltaTime;
            }
            Velocity.Total += direction * -dragPercent;
        }

        private void CalculateSpeed(in float deltaTime)
        {
            if (!allowDebug) return;

            _lastPosition = _currentPosition;
            _currentPosition = target.localPosition;
            float distance = Vector3.Distance(_currentPosition, _lastPosition);
            Vector3 velocity = target.InverseTransformDirection(Velocity.Total);
            Speed.Normalised = distance / deltaTime;
            Speed.Forward = (velocity.z / deltaTime).Abs();
            Speed.Strafe = (velocity.x / deltaTime).Abs();
            Speed.Hover = (velocity.y / deltaTime).Abs();

            string log = $"\nNormalised spd: {Speed.Normalised}\n" +
                         $"Forward spd: {Speed.Forward}\n" +
                         $"Strafe (lft,rght) spd: {Speed.Strafe}\n" +
                         $"Hover (up,down) spd: {Speed.Hover}";
            DebugLog(log);
        }

        private void Move(in float deltaTime) => target.position += Velocity.Total * deltaTime;

        private void LerpCummulatedAcceleration(in float deltaTime)
        {
            if (IsMoving) Accel.CummulatedAcceleration += Accel.CummulationSpeed * deltaTime;
            else Accel.CummulatedAcceleration -= Accel.CummulationSpeed * deltaTime;
            Accel.CummulatedAcceleration = Mathf.Clamp(Accel.CummulatedAcceleration, 0.0f, Accel.MaxCummulation);
        }

        #endregion

        #region Shorthands

        private float VerticalInputSpeed => Input.VerticalAxis * Speed.InputForward;
        private float HorizontalInputSpeed => Input.HorizontalAxis * Speed.InputStrafe;
        private float HoverInputSpeed => Input.HoverAxis * Speed.InputHover;
        private float BoostInputSpeed => /*allowBoost*/ false.AsFloat() * Input.BoostAxis * Accel.Boost + 1.0f;
        public override float SqrSpeed => Velocity.SquareSpeed;

        private bool IsMoving => VerticalInputSpeed.Abs() > float.Epsilon || HorizontalInputSpeed.Abs() > float.Epsilon || HoverInputSpeed.Abs() > float.Epsilon;

        private Vector3 ForwardVelocity => target.forward * _currentForwardSpeed;
        private Vector3 StrafeVelocity => target.right * _currentStrafeSpeed;
        private Vector3 HoverVelocity => target.up * _currentHoverSpeed;

        #endregion
    }
}