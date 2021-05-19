using Hadal.Inputs;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.Locomotion
{
    public class PlayerMovementF : Mover
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
            /*      LerpCummulatedAcceleration(deltaTime);
                   HandleAcceleration(deltaTime);
                   AddVelocity();
                   LoseVelocity(deltaTime);
                   Move(deltaTime);
                   CalculateSpeed(deltaTime);*/
        }

        public override void DoFixedUpdate(in float fixedDeltaTime)
        {
            if (!allowUpdate) return;
            HandleAcceleration(fixedDeltaTime);
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
            _currentForwardSpeed = VerticalInputSpeed * BoostInputSpeed * Accel.Forward * deltaTime;
            _currentStrafeSpeed = HorizontalInputSpeed * BoostInputSpeed * Accel.Strafe * deltaTime;
            _currentHoverSpeed = HoverInputSpeed * BoostInputSpeed * Accel.Hover * deltaTime;

            Vector3 moveForce = target.forward * _currentForwardSpeed + target.right * _currentStrafeSpeed + target.up * _currentHoverSpeed ;
            rigidBody.AddForce(moveForce * 20);
            Debug.Log("Force Added " + moveForce.magnitude);
        }

        #endregion

        #region Shorthands
        private float VerticalInputSpeed => Input.VerticalAxis * Speed.InputForward;
        private float HorizontalInputSpeed => Input.HorizontalAxis * Speed.InputStrafe;
        private float HoverInputSpeed => Input.HoverAxis * Speed.InputHover;
        private float BoostInputSpeed => /*allowBoost*/ false.AsFloat() * Input.BoostAxis * Accel.Boost + 1.0f;
        public override float SqrSpeed => Velocity.SquareSpeed;
        private Rigidbody rigidBody => target.GetComponent<Rigidbody>();
        #endregion
    }
}
