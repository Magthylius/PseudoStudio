using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Hadal.Player.Aesthetics
{
    public class SubmarineFinBehaviour : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] float tweenSpeed = 20f;

        [Header("Rotation Values")]
        [SerializeField] Quaternion originalRotation;
        [SerializeField] Quaternion forwardRotation;
        [SerializeField] Quaternion backwardRotation;
        [SerializeField] Quaternion upwardRotation;
        [SerializeField] Quaternion downwardRotation;
        [SerializeField] Quaternion turnLeftRotation;
        [SerializeField] Quaternion turnRightRotation;

        Quaternion targetRotation;

        int sl_TotalMovementRot;

        void Start()
        {
            targetRotation = originalRotation;
            sl_TotalMovementRot = DebugManager.Instance.CreateScreenLogger();
        }

        void Update()
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, tweenSpeed * Time.deltaTime);
        }

        public void UpdateMovement(Vector3 normalizedVector)
        {
            if (normalizedVector.sqrMagnitude > 0)
            {
                Vector3 totalMovement = Vector3.zero;

                //! forwards backwards
                /*if (movementVector.z < 0f) totalMovement += movementVector.x * forwardRotation.eulerAngles;
                else if (movementVector.z > 0f) totalMovement += Mathf.Abs(movementVector.x) * backwardRotation.eulerAngles;

                if (movementVector.y < 0f) totalMovement += movementVector.y * upwardRotation.eulerAngles;
                else if (movementVector.y > 0f) totalMovement += Mathf.Abs(movementVector.y) * downwardRotation.eulerAngles;*/

                float zProg = (normalizedVector.z + 1) * 0.5f;
                Vector3 zAngle = Vector3.Lerp(backwardRotation.eulerAngles, forwardRotation.eulerAngles, zProg);

                targetRotation = Quaternion.Euler(zAngle);
                DebugManager.Instance.SLog(sl_TotalMovementRot, "Rot", zAngle);
            }
            else
            {
                targetRotation = originalRotation;

            }
        }

        [Button("Set Original Rotation")]
        void SetOriginalRotation() => originalRotation = transform.rotation;
        [Button("Reset To Original Rotation")]
        public void ResetToOriginalRotation() => transform.rotation = originalRotation;

    }
}
