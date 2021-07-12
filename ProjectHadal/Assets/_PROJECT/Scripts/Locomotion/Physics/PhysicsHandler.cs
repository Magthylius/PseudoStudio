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
        [SerializeField] private float density;
        [SerializeField] private float drag;
        [SerializeField] private float weightForce;
        [SerializeField] private float buoyantForce;
        [SerializeField] private float dragForce;

        [Header("Drag Raycasting")]
        [SerializeField] private Vector3 moveDirection;
        [SerializeField] private LayerMask rayCastLayerMask;
        [SerializeField] private int testHitCount;
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

        void OnDrawGizmos()
        {
          /*  Vector3 v1 = Vector3.Cross(moveDirection, Vector3.up).normalized;
            Vector3 v2 = Vector3.Cross(moveDirection, v1).normalized;
            Gizmos.DrawLine(transform.position, transform.position + moveDirection * 10);

            Vector3 movePoint = transform.position + moveDirection * 10f;
            Gizmos.DrawLine(movePoint, movePoint + v1 * 10);
            Gizmos.DrawLine(movePoint, movePoint + v2 * 10);

            //print(Vector3.Distance(movePoint, movePoint + v1 * 10));
            int width = 10;
            int height = 10;

            for (float x = -width; x <= width; x++)
            {
                for (float y = -width; y <= width; y++)
                {
                    Vector3 start = movePoint + (v1 * x) + (v2 * y);
                    Gizmos.DrawLine(start - moveDirection, start);
                }
            }*/
        }
        #endregion

        #region Private Methods
        public void SetUpRigidBody()
        {
            rigidBody.mass = weightForce / 9.8f;
            rigidBody.useGravity = true;
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
                buoyantForce = volume * density * 10;
                print(buoyantForce);
                return;
            }
        }

        public void CalculateWaterDrag(Vector3 moveVector)
        {
            testHitCount = 0;
            moveDirection = moveVector.normalized;
            Vector3 v1 = Vector3.Cross(moveDirection, Vector3.up).normalized;
            Vector3 v2 = Vector3.Cross(moveDirection, v1).normalized;
            
            Vector3 movePoint = transform.position + moveDirection * 10f;
            int width = 10;
            int height = 10;

            for (float x = -width; x <= width; x++)
            {
                for (float y = -width; y <= width; y++)
                {
                    Vector3 start = movePoint + (v1 * x) + (v2 * y);
                    /*Debug.DrawRay(start, - moveDirection * 10, Color.green);*/

                    if (Physics.Raycast(start,  - moveDirection, out aimHit,50
                                , rayCastLayerMask, QueryTriggerInteraction.Ignore))
                    {
                        Debug.DrawRay(start, -moveDirection * 10, Color.red);
                        testHitCount++;
                    }
                }
            }
                                   //1/2  *  Density * Vector^2                                        * area
            Vector3 finalDragForce = 0.5f * density * Vector3.SqrMagnitude(moveVector) * moveDirection * testHitCount * 0.099f;
            rigidBody.AddForce(-finalDragForce, ForceMode.Force);
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
