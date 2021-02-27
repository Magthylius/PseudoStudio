//Created by Harry, E: Jon
using UnityEngine;
using NaughtyAttributes;

namespace Hadal.PostProcess
{
    public class DynamicDepthOfField : MonoBehaviour // put on the player
    {
        PostProcessingManager ppManager; // assign post processingmanager to this
        DebugManager debugManager;

        [Header("Dynamic Depth of Field")]
        public Transform postprocessFirePoint;
        public float focusSpeed = 8;
        public float maxFocusDistance = 100;

        Ray raycast;
        RaycastHit hit;
        [ReadOnly] public bool isHit;
        float hitDistance;

        int debugFocalDistance;

        void Start()
        {
            ppManager = PostProcessingManager.Instance;
            debugManager = DebugManager.Instance;

            debugFocalDistance = debugManager.CreateScreenLogger();
        }

        void Update()
        {
            raycast = new Ray(postprocessFirePoint.position, postprocessFirePoint.forward * 100);
            isHit = false;

            if (Physics.Raycast(raycast, out hit, maxFocusDistance))
            {
                isHit = true;
                hitDistance = hit.distance;
            }
            else
            {
                if (hitDistance < 100f)
                    hitDistance++;
            }
            ppManager.EditDepthOfField(hitDistance, focusSpeed);

            debugManager.SLog(debugFocalDistance, "FocalDistance", hitDistance);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(raycast);
        }
    }
}
