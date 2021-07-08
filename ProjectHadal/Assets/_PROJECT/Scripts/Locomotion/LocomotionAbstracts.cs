using Hadal.Inputs;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.Locomotion
{
	[System.Serializable]
    public abstract class Mover : MonoBehaviourDebug
    {
        public IMovementInput Input { get; set; }
        public SpeedInfo Speed;
        public AccelerationInfo Accel;
        public VelocityInfo Velocity;
        protected Transform target;
        protected float _currentForwardSpeed, _currentStrafeSpeed, _currentHoverSpeed;
        private bool _allowUpdate = false;
        protected bool allowUpdate
        {
            get => _allowUpdate;
            set
            {
                _allowUpdate = value;
                // $"Allow update changed to {value}, for player name: {gameObject.transform.parent.gameObject.name}".Msg();
            }
        }
        public abstract float SqrSpeed { get; }
        public abstract void Initialise(Transform transform);
        public abstract void DoUpdate(in float deltaTime);
        public abstract void DoFixedUpdate(in float fixedDeltaTime);    
        public abstract void DoLateUpdate(in float deltaTime);
        public virtual void Enable() => allowUpdate = true;
        public virtual void Disable() => allowUpdate = false;
        public void ToggleEnablility() => allowUpdate = !allowUpdate;
        public void ToggleEnablility(bool isTrue)
        {
            // print("Toggle allow Update !");
            allowUpdate = isTrue;
        }

        //Delete later
        protected bool allowBoost;
        public void EnableBoost() => allowBoost = true;
        public void DisableBoost() => allowBoost = false;
    }
    [System.Serializable]
    public abstract class Rotator : MonoBehaviour
    {
        public IRotationInput Input { get; set; }
        public RotationInfo Rotary;
        protected Transform target;
        protected bool allowUpdate;

        public abstract void Initialise(Transform transform);
        public abstract void DoUpdate(in float deltaTime);
        public abstract void DoFixedUpdate(in float fixedDeltaTime);
        public abstract void DoLateUpdate(in float deltaTime);
        public void Enable() => allowUpdate = true;
        public void Disable() => allowUpdate = false;
        public void ToggleEnablility() => allowUpdate = !allowUpdate;

        public Quaternion localRotation => target.localRotation;
        public Quaternion rotation => target.rotation;

    }
}