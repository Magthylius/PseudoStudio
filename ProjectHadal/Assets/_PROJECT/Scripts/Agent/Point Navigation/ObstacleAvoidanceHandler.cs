using System;
using System.Collections.Generic;
using Tenshi;
using UnityEngine;

namespace Hadal.AI
{
    public class ObstacleAvoidanceHandler : MonoBehaviour
    {
        [SerializeField] private PointNavigationHandler navigator;
        private SphereCollider cCollider;

        private void OnValidate()
        {
            if (cCollider == null) cCollider = GetComponent<SphereCollider>();
            cCollider.radius = navigator.ObstacleDetectionRadius;
        }

        private void Awake()
        {
            cCollider = GetComponent<SphereCollider>();
            navigator.OnObstacleDetectRadiusChange += UpdateData;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ShouldCollide(other))
                navigator.AddRepulsionPoint(other.ClosestPointOnBounds(navigator.PilotTransform.position));
        }

        private void OnTriggerStay(Collider other)
        {
            if (ShouldCollide(other))
                navigator.AddRepulsionPoint(other.ClosestPointOnBounds(navigator.PilotTransform.position));
        }

        public void UpdateData(float radius)
        {
            cCollider.radius = radius;
        }
        
        private bool ShouldCollide(Collider other)
            => navigator.ObstacleTimerReached && other.gameObject.layer.IsAMatchingMask(navigator.GetObstacleMask);
    }
}
