using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Hadal.Player.Aesthetics
{
    public class SubmarineFinBehaviour : MonoBehaviour
    {
        [SerializeField] Quaternion originalRotation;
        [SerializeField] Quaternion forwardRotation;
        [SerializeField] Quaternion backwardRotation;
        [SerializeField] Quaternion upwardRotation;
        [SerializeField] Quaternion downwardRotation;
        [SerializeField] Quaternion turnLeftRotation;
        [SerializeField] Quaternion turnRightRotation;

        void Start()
        {
        
        }

        void Update()
        {
        
        }

        [Button("Set Original Rotation")]
        public void SetOriginalRotation() => originalRotation = transform.rotation;
        [Button("Reset To Original Rotation")]
        public void ResetToOriginalRotation() => transform.rotation = originalRotation;

    }
}
