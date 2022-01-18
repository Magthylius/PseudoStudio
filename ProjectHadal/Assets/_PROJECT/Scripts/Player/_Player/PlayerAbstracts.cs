using Hadal.Locomotion;
using NaughtyAttributes;
using Photon.Pun;
using UnityEngine;

//Created by Jet
namespace Hadal
{
    [System.Serializable]
    public abstract class Controller : MonoBehaviourPunCallbacks
    {
        [Foldout("Main"), SerializeField] protected Mover mover;
        [Foldout("Main"), SerializeField] protected Rotator rotator;
        [Foldout("Main"), SerializeField] protected Transform pTrans;

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

        protected virtual void FixedUpdate()
        {
            mover.DoFixedUpdate(FixedDeltaTime);
            rotator.DoFixedUpdate(FixedDeltaTime);
        }

        protected virtual void LateUpdate()
        {
            mover.DoLateUpdate(DeltaTime);
            rotator.DoLateUpdate(DeltaTime);
        }

        public float DeltaTime => Time.deltaTime;
        public float FixedDeltaTime => Time.fixedDeltaTime;
        public float SqrSpeed => mover.SqrSpeed;

        public Rotator Rotator => rotator;
        public Mover Mover => mover;
    }
    [System.Serializable]
    public abstract class CameraController : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] protected Camera selfCamera;
    }
}