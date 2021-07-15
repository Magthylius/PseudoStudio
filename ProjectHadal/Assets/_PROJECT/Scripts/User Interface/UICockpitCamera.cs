using System;
using System.Collections;
using System.Collections.Generic;
using Hadal.Inputs;
using NaughtyAttributes;
using UnityEngine;

namespace Hadal.UI
{
    public class UICockpitCamera : MonoBehaviour
    {
        public Camera cockpitCamera;
        public float maxAngles = 10f;
        public float inputSensitivity = 1f;
        public float maxInputAxisClamp = 1f;
        public float lerpSpeed = 2f;

        private bool isInitialized = false;
        private Quaternion cameraLocalTargetQuat;
        private Quaternion cameraOriginalLocalQuat;
        private Vector3 cameraLocalPosition;
        private IRotationInput playerRotInput;
        
        private void LateUpdate()
        {
            if (!isInitialized) return;

            Vector3 input = playerRotInput.AllInputClamped(-maxInputAxisClamp, maxInputAxisClamp);
            
            float pitch = Mathf.Lerp(0f, maxAngles, Mathf.Abs(-input.y) * inputSensitivity);
            float yaw = Mathf.Lerp(0f, maxAngles, Mathf.Abs(input.x) * inputSensitivity);
            pitch *= Mathf.Sign(-input.y);
            yaw *= Mathf.Sign(input.x);

            cameraLocalTargetQuat = cameraOriginalLocalQuat * Quaternion.Euler(pitch, yaw, 0f);
            cockpitCamera.transform.localRotation = Quaternion.Lerp(cockpitCamera.transform.localRotation, cameraLocalTargetQuat, lerpSpeed * Time.deltaTime);
        }

        public void InjectDependencies(IRotationInput input)
        {
            playerRotInput = input;
            cameraOriginalLocalQuat = cockpitCamera.transform.localRotation;
            //cameraLocalPosition = cockpitCamera.transform.localPosition;
            
            isInitialized = true;
        }

    }
}
