using Hadal.Inputs;
using UnityEngine;

namespace Hadal.Locomotion
{
	[System.Serializable]
    public abstract class Mover : MonoBehaviour
    {
        public IMovementInput Input { get; set; }
        public SpeedInfo Speed;
        public AccelerationInfo Accel;
        public VelocityInfo Velocity;
        protected Transform target;
        protected float _currentForwardSpeed, _currentStrafeSpeed, _currentHoverSpeed;
        public abstract float SqrSpeed { get; }
        public abstract void Initialise(Transform transform);
        public abstract void DoUpdate(in float deltaTime);
    }
    [System.Serializable]
    public abstract class Rotator : MonoBehaviour
    {
        public IRotationInput Input { get; set; }
        public RotationInfo Rotary;
        protected Transform target;
        public abstract void Initialise(Transform transform);
        public abstract void DoUpdate(in float deltaTime);

        public Quaternion localRotation => target.localRotation;
        public Quaternion rotation => target.rotation;
    }
}