using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Locomotion
{
    public class PhysicsHandler : MonoBehaviour
    {
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private Collider objectCollider;

        [Header("Physic Stimulation")]
        [SerializeField] private float drag;
        [SerializeField] private float weightForce;
        [SerializeField] private float buoyantForce;
        [SerializeField] private float dragForce;

        [Header("Drag Raycasting")]
        [SerializeField] private int rayCastLayerMask;
        RaycastHit aimHit;
        #region Unity LifeCycle
        void Start()
        {
            SetUpRigidBody();
            CalculateBuoyantForce();
        }
    
        void FixedUpdate()
        {
            rigidBody.AddForce(buoyantForce * Vector3.up, ForceMode.Force);
        }
        #endregion

        #region Private Methods
        private void SetUpRigidBody()
        {
            rigidBody.mass = weightForce / 9.8f;
            rigidBody.useGravity = true;
            rayCastLayerMask = rigidBody.gameObject.layer;
        }

        private void CalculateBuoyantForce()
        {
            if(objectCollider is BoxCollider)
            {
                BoxCollider collider = (BoxCollider)objectCollider;
                print(collider.size);
                return;
            }
            else if(objectCollider is CapsuleCollider)
            {
                CapsuleCollider collider = (CapsuleCollider)objectCollider;
                float volume = collider.height * Mathf.Pow(2, (collider.radius * 2));
                print(volume);
                float density = 1029;
                buoyantForce = volume * density * 10;
                print(buoyantForce);
                return;
            }
        }

        public void CalculateWaterDrag(Vector3 moveVector)
        {
            /*if (Physics.Raycast(aimPoint.position, moveVector, out aimHit,
                                Mathf.Infinity, rayCastLayerMask, QueryTriggerInteraction.Ignore))
            {

            }*/
        }
        #endregion

        #region Acessors
        public float Drag { get => drag; set => drag = value; }
        public float WeightForce { get => weightForce; set => weightForce = value; }
        public float BuoyantForce { get => buoyantForce; set => buoyantForce = value; }
        public float DragForce { get => dragForce; set => dragForce = value; }
        #endregion
    }
}
