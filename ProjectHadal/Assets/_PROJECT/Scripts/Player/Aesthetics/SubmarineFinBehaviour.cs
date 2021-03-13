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

        void Start()
        {
            targetRotation = originalRotation;
        }

        void Update()
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, tweenSpeed * Time.deltaTime);
        }

        public void UpdateMovement(Vector3 movementVector)
        {
            if (movementVector.sqrMagnitude > 0)
            {
                Vector3 totalMovement = Vector3.zero;

                //! forwards backwards
                if (movementVector.z < 0f) totalMovement += movementVector.x * forwardRotation.eulerAngles;
                else if (movementVector.z > 0f) totalMovement += Mathf.Abs(movementVector.x) * backwardRotation.eulerAngles;

                if (movementVector.y < 0f) totalMovement += movementVector.y * upwardRotation.eulerAngles;
                else if (movementVector.y > 0f) totalMovement += Mathf.Abs(movementVector.y) * downwardRotation.eulerAngles;

                targetRotation = Quaternion.Euler(totalMovement.normalized * 360f);
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
