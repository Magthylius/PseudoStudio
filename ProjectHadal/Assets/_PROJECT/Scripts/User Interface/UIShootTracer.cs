using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.UI
{
    public class UIShootTracer : MonoBehaviour
    {
        [Header("References")]
        public LineRenderer line;
        public Light hitLight;
        public Transform lineStartTransform;

        private Transform playerCamera;
 
        private bool isActive = false;
        private RaycastHit forwardHit;
        private LayerMask rayIgnoreMask;

        [Header("Color settings")] 
        public Material redMat;
        public Material blueMat;

        private void Start()
        {
            //Deactivate();
            Vector3 startPos = lineStartTransform.position;
            line.SetPositions(new [] {startPos, startPos});
            hitLight.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (playerCamera == null)
            {
                Debug.LogWarning("Player camera is null on shoot tracer!");
                return;
            }

            Physics.Raycast(playerCamera.position, playerCamera.forward, out forwardHit,
                Mathf.Infinity, ~rayIgnoreMask, QueryTriggerInteraction.Ignore);

            if (!isActive) return;
            line.SetPositions(new [] {lineStartTransform.position, forwardHit.point});
            hitLight.transform.position = forwardHit.point;
        }

        public void InjectDependencies(Camera pCamera) => playerCamera = pCamera.transform;

        public void ToBlue() => line.material = blueMat;
        public void ToRed() => line.material = redMat;
        
        public void Activate()
        {
            isActive = true;
            hitLight.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            isActive = false;
            //! make it hide itself
            line.SetPosition(1, line.GetPosition(0));
            
            hitLight.gameObject.SetActive(false);
            hitLight.transform.position = lineStartTransform.position;
        }

        public bool IsActive => isActive;

        public Vector3 HitPoint => forwardHit.point;
        public float HitDistance => forwardHit.distance;
    }

}