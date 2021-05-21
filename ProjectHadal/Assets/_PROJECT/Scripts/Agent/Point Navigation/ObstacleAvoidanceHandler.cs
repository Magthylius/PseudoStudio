using UnityEngine;

namespace Hadal.AI
{
    public class ObstacleAvoidanceHandler : MonoBehaviour
    {
        [SerializeField] private PointNavigation navigator;
        private SphereCollider sCollider;

        private void Awake()
        {
            sCollider = GetComponent<SphereCollider>();
            sCollider.radius = navigator.ObstacleDetectionRadius;
        }

        private void OnTriggerEnter(Collider other)
        {
            navigator.AddRepulsionPoint(other.ClosestPointOnBounds(navigator.PilotTransform.position));
        }
    }
}
