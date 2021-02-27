//Created by Harry
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.PostProcess
{
    public class DynamicDepthOfField : MonoBehaviour // put on the player
    {
        public PostProcessingManager Instance; // assign post processingmanager to this

        [Header("Dynamic Depth of Field")]
        Ray raycast;
        RaycastHit hit;
        [SerializeField]bool isHit;
        float hitDistance;
        public float focusSpeed = 8;
        public float maxFocusDistance = 100;

        private void Start()
        {

        }

        void Update()
        {
            raycast = new Ray(transform.position, transform.forward * 100);
            isHit = false;

            if (Physics.Raycast(raycast, out hit, maxFocusDistance))
            {
                isHit = true;
                hitDistance = Vector3.Distance(transform.position, hit.point);
            }
            else
            {
                if (hitDistance < 100f)
                    hitDistance++;
            }
            Instance.EditDepthOfField(hitDistance, focusSpeed);
        }
    }
}
