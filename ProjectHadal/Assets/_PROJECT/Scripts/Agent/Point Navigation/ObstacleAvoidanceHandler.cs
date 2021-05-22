using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
{
    public class ObstacleAvoidanceHandler : MonoBehaviour
    {
        [SerializeField] private PointNavigation navigator;
        private SphereCollider cCollider;
        
        private void Awake()
        {
            cCollider = GetComponent<SphereCollider>();
            cCollider.radius = navigator.ObstacleDetectionRadius;
        }

        private void OnTriggerEnter(Collider other)
        {
            navigator.AddRepulsionPoint(other.ClosestPointOnBounds(navigator.PilotTransform.position));
        }

        private void OnTriggerStay(Collider other)
        {
            navigator.AddRepulsionPoint(other.ClosestPointOnBounds(navigator.PilotTransform.position));
        }
    }
}
