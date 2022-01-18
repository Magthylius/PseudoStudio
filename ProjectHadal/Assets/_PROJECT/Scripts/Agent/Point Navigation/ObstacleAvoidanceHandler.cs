using System;
using System.Collections.Generic;
using Photon.Pun;
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
            if (navigator != null) cCollider.radius = navigator.ObstacleDetectionRadius;
        }

        private void Awake()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;
            cCollider = GetComponent<SphereCollider>();
            navigator.OnObstacleDetectRadiusChange += UpdateData;
        }

        private void OnDestroy()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;
            
            navigator.OnObstacleDetectRadiusChange -= UpdateData;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ShouldCollide(other) && navigator.CanMove)
                navigator.AddRepulsionPoint(other.ClosestPointOnBounds(navigator.PilotTransform.position));
        }

        private void OnTriggerStay(Collider other)
        {
            if (ShouldCollide(other) && navigator.CanMove)
                navigator.AddRepulsionPoint(other.ClosestPointOnBounds(navigator.PilotTransform.position));
        }

        public void UpdateData(float radius)
        {
            cCollider.radius = radius;
        }
        
        private bool ShouldCollide(Collider other)
            => navigator.ObstacleTimerReached
            && PhotonNetwork.IsMasterClient
            && other.gameObject.layer.IsAMatchingMask(navigator.GetObstacleMask);
    }
}
