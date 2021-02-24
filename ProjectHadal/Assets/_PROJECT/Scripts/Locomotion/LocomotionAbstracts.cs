using Hadal.Inputs;
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
        protected bool allowUpdate;
        public abstract float SqrSpeed { get; }
        public abstract void Initialise(Transform transform);
        public abstract void DoUpdate(in float deltaTime);
        public void Enable() => allowUpdate = true;
        public void Disable() => allowUpdate = false;
        public void ToggleEnablility() => allowUpdate = !allowUpdate;
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
        public void Enable() => allowUpdate = true;
        public void Disable() => allowUpdate = false;
        public void ToggleEnablility() => allowUpdate = !allowUpdate;

        public Quaternion localRotation => target.localRotation;
        public Quaternion rotation => target.rotation;
    }
}