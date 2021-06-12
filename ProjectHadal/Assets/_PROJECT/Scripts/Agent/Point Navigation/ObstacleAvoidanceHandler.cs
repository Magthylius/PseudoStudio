using System;
using System.Collections.Generic;
using Tenshi;
using UnityEngine;

namespace Hadal.AI
{
    public class ObstacleAvoidanceHandler : MonoBehaviour
    {
        [SerializeField] private PointNavigationHandler navigator;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private LayerMask wallMask;
        private SphereCollider cCollider;

        private void OnValidate()
        {
            cCollider = GetComponent<SphereCollider>();
            cCollider.radius = navigator.ObstacleDetectionRadius;
        }

        private void Awake()
        {
            cCollider = GetComponent<SphereCollider>();
            cCollider.radius = navigator.ObstacleDetectionRadius;
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

        private bool ShouldCollide(Collider other)
            => navigator.ObstacleTimerReached;
            // || other.gameObject.layer == obstacleMask.ToLayer()
            // || other.gameObject.layer == wallMask.ToLayer();
    }
}
