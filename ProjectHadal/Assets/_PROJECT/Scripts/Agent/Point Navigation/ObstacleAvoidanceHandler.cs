using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
{
    public class ObstacleAvoidanceHandler : MonoBehaviour
    {
        [SerializeField] private PointNavigationHandler navigator;
        private SphereCollider cCollider;
        
        private void Awake()
        {
            cCollider = GetComponent<SphereCollider>();
            cCollider.radius = navigator.ObstacleDetectionRadius;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!navigator.ObstacleTimerReached) return;
            navigator.AddRepulsionPoint(other.ClosestPointOnBounds(navigator.PilotTransform.position));
        }

        private void OnTriggerStay(Collider other)
        {
            if (!navigator.ObstacleTimerReached) return;
            navigator.AddRepulsionPoint(other.ClosestPointOnBounds(navigator.PilotTransform.position));
        }
    }
}
