using Hadal.Inputs;
using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.Locomotion
{
    public class PlayerMovementH : Mover
    {
        [Header("Debug"), SerializeField] private string debugKey;
        [SerializeField] private Rigidbody rigidBody;

        private Vector3 _lastPosition;
        private Vector3 _currentPosition;
        private bool _isLocal = true;
        private bool _isEnabled = false;

        [SerializeField, ReadOnly] private float drag;

        public float CalculatedDrag => drag;
        public Rigidbody Rigidbody { get => rigidBody; set => rigidBody = value; }

        public override void Initialise(Transform target)
        {
            this.target = target;
            Enable();
            EnableBoost();
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
            
        }

        public override void DoFixedUpdate(in float fixedDeltaTime)
        {
            if (!allowUpdate) return;

            HandleAcceleration(fixedDeltaTime);
            CalculateSpeed();
        }

        public override void DoLateUpdate(in float deltaTime)
        {

        }

        public void SetIsLocal(bool state) => _isLocal = state;

        static readonly IMovementInput DefaultInputs = new RawMovementInput();
        static readonly IMovementInput DisabledInputs = new EmptyMovementInput();

        public override void Enable()
        {
            //$"Enable is called".Warn();
            if (_isEnabled)
                return;

            //this cant be enabled because when the game starts
            //base.Enable will allow the allowUpdate.
            //which allows the player to move while loading.
            base.Enable();

            _isEnabled = true;
            Input = DefaultInputs;
            
            drag = Accel.MaxCummulation / Speed.Max;
            if (rigidBody != null) rigidBody.drag = (drag / (drag * Time.fixedDeltaTime + 1));

            // CalculateDrag();
            // if (rigidBody != null) rigidBody.drag = GetModifiedDrag();
        }

        public override void Disable()
        {
            //$"Disable is called".Warn();
            _isEnabled = false;
            Input = DisabledInputs;
        }

        public void CalculateDrag() => drag = Accel.MaxCummulation / Speed.Max;
        public float GetModifiedDrag() => drag / (drag * Time.fixedDeltaTime + 1);

        #region Private Methods
        private void HandleAcceleration(in float deltaTime)
        {
            _currentForwardSpeed = VerticalInputSpeed  * Accel.Forward * deltaTime;
            _currentStrafeSpeed = HorizontalInputSpeed  * Accel.Strafe * deltaTime;
            _currentHoverSpeed = HoverInputSpeed  * Accel.Hover * deltaTime;

            Vector3 moveForce = (target.forward * _currentForwardSpeed + target.right * _currentStrafeSpeed + target.up * _currentHoverSpeed) * 100 ;

            if(moveForce.magnitude > Accel.MaxCummulation)
            {
                moveForce = moveForce.normalized * Accel.MaxCummulation;
            }

            rigidBody.AddForce(moveForce, ForceMode.Force);
        }

        private void CalculateSpeed()
        {
            _lastPosition = _currentPosition;
            _currentPosition = target.localPosition;
            float distance = Vector3.Distance(_currentPosition, _lastPosition);
            Speed.Normalised = distance / Time.deltaTime;

            /*Speed.Normalised = rigidBody.velocity.magnitude;*/
            Speed.Forward = rigidBody.velocity.x;
            Speed.Strafe = rigidBody.velocity.z;
            Speed.Hover = rigidBody.velocity.y;

        }
        #endregion

        #region Shorthands
        private float VerticalInputSpeed => Input.VerticalAxis * Speed.InputForward;
        private float HorizontalInputSpeed => Input.HorizontalAxis * Speed.InputStrafe;
        private float HoverInputSpeed => Input.HoverAxis * Speed.InputHover;
        private float BoostInputSpeed => /*allowBoost*/ false.AsFloat() * Input.BoostAxis * Accel.Boost + 1.0f;
        public override float SqrSpeed => Velocity.SquareSpeed;
        #endregion
    }
}
