using Hadal.Controls;
using Photon.Pun;
using UnityEngine;

//Created by Jet
namespace Hadal
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
    }
    [System.Serializable]
    public abstract class Controller : MonoBehaviourPunCallbacks
    {
        [SerializeField] protected Mover mover;
        [SerializeField] protected Rotator rotator;
        [SerializeField] protected Transform pTrans;

        protected virtual void Awake()
        {
            mover.Initialise(pTrans);
            rotator.Initialise(pTrans);
        }

        protected virtual void Update()
        {
            mover.DoUpdate(DeltaTime);
            rotator.DoUpdate(DeltaTime);
        }

        public float DeltaTime => Time.deltaTime;
        public float SqrSpeed => mover.SqrSpeed;
    }
    [System.Serializable]
    public abstract class CameraController : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] protected Camera selfCamera;
    }
}