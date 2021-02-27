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
        [MinMaxSlider(0, 100f)] public Vector2 focusDistanceRange;
        [MinMaxSlider(1, 300f)] public Vector2 focalLengthRange;

        Ray raycast;
        RaycastHit hit;
        [ReadOnly, SerializeField] bool isHit;

        float focusDistance;

        int sLog_FocusDistance;
        int sLog_FocalLength;

        void Start()
        {
            ppManager = PostProcessingManager.Instance;
            debugManager = DebugManager.Instance;

            sLog_FocusDistance = debugManager.CreateScreenLogger();
            sLog_FocalLength = debugManager.CreateScreenLogger();
        }

        void Update()
        {
            raycast = new Ray(postprocessFirePoint.position, postprocessFirePoint.forward * 100);
            isHit = false;

            if (Physics.Raycast(raycast, out hit, focusDistanceRange.y))
            {
                isHit = true;
                focusDistance = hit.distance;

                if (focusDistance < focusDistanceRange.x) focusDistance = focusDistanceRange.x;
            }
            else
            {
                if (focusDistance < focusDistanceRange.y)
                    focusDistance++;
            }

            float focalRatio = focusDistance / focusDistanceRange.x;
            float focalLength = Mathf.Lerp(focalLengthRange.x, focalLengthRange.y, focalRatio);

            ppManager.EditDepthOfField(focusDistance, focalLength, focusSpeed);

            debugManager.SLog(sLog_FocusDistance, "FocalDistance", focusDistance);
            debugManager.SLog(sLog_FocalLength, "FocalLength", focalLength);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(raycast);
        }
    }
}
