using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Hadal.Player.Aesthetics
{
    public class SubmarineFinBehaviour : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] bool invertedFin = false;
        [SerializeField] Quaternion currentQuat;

        [Header("Rotation Values")]
        [SerializeField] Quaternion originalRotation;
        [SerializeField] Quaternion forwardRotation;
        [SerializeField] Quaternion backwardRotation;
        [SerializeField] Quaternion upwardRotation;
        [SerializeField] Quaternion downwardRotation;
        [SerializeField] Quaternion turnLeftRotation;
        [SerializeField] Quaternion turnRightRotation;

        float tweenSpeed = 20f;
        Quaternion targetRotation;

        

        void OnValidate()
        {
            currentQuat = transform.localRotation;
        }

        void Start()
        {
            targetRotation = originalRotation;
        }

        void Update()
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, tweenSpeed * Time.deltaTime);
        }

        public void UpdateMovement(Vector3 normalizedVector, float magnitude)
        {
            //targetRotation = Quaternion.identity;

            if (normalizedVector.sqrMagnitude > 0.2f)
            {
                //Vector3 totalMovement = Vector3.zero;

                //! forwards backwards
                if (normalizedVector.z < 0f)
                    targetRotation *= forwardRotation;
                else if (normalizedVector.z > 0f)
                    targetRotation *= backwardRotation;

                //! forwards backwards
                /*if (movementVector.z < 0f) totalMovement += movementVector.x * forwardRotation.eulerAngles;
                else if (movementVector.z > 0f) totalMovement += Mathf.Abs(movementVector.x) * backwardRotation.eulerAngles;

                if (movementVector.y < 0f) totalMovement += movementVector.y * upwardRotation.eulerAngles;
                else if (movementVector.y > 0f) totalMovement += Mathf.Abs(movementVector.y) * downwardRotation.eulerAngles;*/

                //float zProg = (normalizedVector.z + 1f) * 0.5f;
                //Vector3 zAngle = Vector3.Lerp(backwardRotation.eulerAngles, forwardRotation.eulerAngles, zProg);

                //float yProg = (normalizedVector.y + 1f) * 0.5f;
                //Vector3 yAngle = Vector3.Lerp(downwardRotation.eulerAngles, upwardRotation.eulerAngles, yProg);

                //if (invertedFin)
                //{
                //    totalMovement += zAngle * Mathf.Abs(normalizedVector.z);
                //    totalMovement += yAngle * Mathf.Abs(normalizedVector.y);
                //}
                //else
                //{
                //    totalMovement += zAngle * Mathf.Abs(normalizedVector.z);
                //    totalMovement += yAngle * Mathf.Abs(normalizedVector.y);
                //}

                ////targetRotation = Quaternion.Euler(yAngle) * Quaternion.Euler(zAngle);
                ////targetRotation = Quaternion.Lerp(Quaternion.Euler(yAngle), Quaternion.Euler(zAngle), (yProg + zProg) * 0.5f);
                //targetRotation = Quaternion.Euler(totalMovement);
                ////DebugManager.Instance.SLog(sl_TotalMovementRot, "Rot", zAngle);
            }
            else
            {
                targetRotation = originalRotation;
            }
        }

        public void SetTweenSpeed(float speed) => tweenSpeed = speed;

        [Button("Set Original Rotation")]
        void SetOriginalRotation() => originalRotation = transform.rotation;
        [Button("Reset To Original Rotation")]
        public void ResetToOriginalRotation() => transform.rotation = originalRotation;

    }
}
