using System.Collections.Generic;
using UnityEngine;

namespace Hadal
{
    /// <summary>
    /// This class depends on a trigger collider in order to detect any rigidbodies
    /// to apply a constant force to.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AccelerationZone : MonoBehaviour
    {
        [SerializeField] private Vector3 worldDirection;
        [SerializeField] private float force;
        [SerializeField] private ForceMode forceMode;
        [SerializeField] private LayerMask affectedLayers;

        private Collider _collider;
        private List<Rigidbody> collidees = new List<Rigidbody>();

        private Vector3 TotalForcePerFrame => worldDirection * force;

        private void Awake()
        {
            worldDirection = worldDirection.normalized;
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            collidees = new List<Rigidbody>();
        }

        private void Update()
        {
            collidees.ForEach(r => r.AddForce(TotalForcePerFrame, forceMode));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsAnAffectedLayer(other.gameObject.layer))
                return;

            Rigidbody rBody = other.gameObject.GetComponent<Rigidbody>();
            if (rBody == null) return;

            if (!collidees.Contains(rBody))
                collidees.Add(rBody);
        }
        private void OnTriggerExit(Collider other)
        {
            if (!IsAnAffectedLayer(other.gameObject.layer))
                return;
            
            Rigidbody rBody = other.gameObject.GetComponent<Rigidbody>();
            if (rBody == null) return;
            
            collidees.Remove(rBody);
        }

        private bool IsAnAffectedLayer(int layerAsInt)
        {
            return ((1 << layerAsInt) & affectedLayers) != 0;
        }
    }
}