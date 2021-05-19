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
            if (!allowUpdate) return;
     /*       LerpCummulatedAcceleration(deltaTime);
            HandleAcceleration(deltaTime);
            AddVelocity();
            LoseVelocity(deltaTime);
            Move(deltaTime);
            CalculateSpeed(deltaTime);*/
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


        #region Shorthands

        public override float SqrSpeed => Velocity.SquareSpeed;

        #endregion
    }
}
