using System.Collections.Generic;
using UnityEngine;

namespace Hadal
{
    /// <summary>
    /// This class depends on a trigger collider in order to detect any rigidbodies
    /// to apply a constant force to.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class AccelerationZone : MonoBehaviour
    {
        [SerializeField] private Vector3 worldDirection;
        [SerializeField] private float force;
		[SerializeField, Range(0f, 4f)] private float exponentFalloffConst;
        [SerializeField] private ForceMode forceMode;
        [SerializeField] private LayerMask affectedLayers;

        /// <summary>
        /// The attached collider to the same game object of this script. Must
        /// always be present, active and isTrigger == true.
        /// </summary>
        private BoxCollider _collider;

        /// <summary>
        /// List of cached rigidbodies that are considered within the trigger collider
        /// so a constant force can be applied to them until they leave the zone.
        /// </summary>
        private List<Rigidbody> collidees = new List<Rigidbody>();
		
		/// <summary>
        /// The highest offset point a rigidbody can be affected by the accel zone. This is relative to the y position of the game object.
        /// </summary>
		private float yPointOffset;
		
        private Vector3 TotalForcePerFrame => worldDirection * force;
		private Vector3 FocusPoint => transform.position;

        private void Awake()
        {
            worldDirection = worldDirection.normalized;
            _collider = GetComponent<BoxCollider>();
            _collider.isTrigger = true;
            collidees = new List<Rigidbody>();
			yPointOffset = _collider.size.y * transform.lossyScale.y;
        }

        private void Update()
        {
            collidees.ForEach(r =>
			{
				float distance = Vector3.Distance(FocusPoint, r.transform.position);
				float multiplier = CalculateExponentialMultiplier(distance);
				
				Vector3 finalForce = TotalForcePerFrame * multiplier;
				//Debug.Log($"dist: {distance}, yOffset: {yPointOffset}, x: {x}, neg2k: {neg2k}, multiplier: {multiplier}, finalForce: {finalForce}");
				
				r.AddForce(finalForce, forceMode);
			});
        }
		
		private float CalculateExponentialMultiplier(float distance)
		{
			float x = Mathf.Clamp(GetNormalisedValue(distance, 0.0f, Mathf.Abs(yPointOffset)), 0f, 1f);
            float neg2k = -(2f * Mathf.Round(exponentFalloffConst));
			float multiplier = Mathf.Pow(x - 2f, neg2k) - Mathf.Pow(2f, neg2k) * (1f - x);
			
			return 1f - multiplier;
		}
		
		private float GetNormalisedValue(float value, float min, float max)
        {
            return (value - min) / (max - min);
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