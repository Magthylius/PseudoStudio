using System.Collections.Generic;
using Tenshi;
using UnityEngine;

namespace Hadal.AI
{
    public class ObstacleAvoidanceHandler : MonoBehaviour
    {
        [SerializeField] private PointNavigationHandler navigator;
        [SerializeField] private LayerMask obstacleMask;
        private SphereCollider cCollider;
        
        private void Awake()
        {
            cCollider = GetComponent<SphereCollider>();
            cCollider.radius = navigator.ObstacleDetectionRadius;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (navigator.ObstacleTimerReached)
                navigator.AddRepulsionPoint(other.ClosestPointOnBounds(navigator.PilotTransform.position));
        }

        private void OnTriggerStay(Collider other)
        {
            if (navigator.ObstacleTimerReached)
                navigator.AddRepulsionPoint(other.ClosestPointOnBounds(navigator.PilotTransform.position));
        }
    }
}
