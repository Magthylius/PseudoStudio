using System;
using UnityEngine;

namespace Hadal.AI.Settings
{
    [CreateAssetMenu(fileName = "New Steering Settings", menuName = "AI/Steering Settings")]
    public class AISteeringSettings : ScriptableObject
    {
        [Header("Steering Settings")]
        [Min(0f), Tooltip("Max amount to clamp the AI's rigidbody velocity.")] public float MaxVelocity;
        [Min(0f), Tooltip("The amount of constant forward force being applied to the AI.")] public float ThrustForce;
        [Min(0f), Tooltip("Attraction force to a custom nav point on a player when the AI is chasing.")] public float AdditionalAttractionForce;
        [Min(0f), Tooltip("Standard attraction force to nav points for basic AI movement.")] public float AttractionForce;
        [Min(0f), Tooltip("Force amount to avoid obstacles when it is moving.")] public float AvoidanceForce;
        [Min(0f), Tooltip("All additional force amount for obstacle avoidance. The closer the AI gets to an obstacle, the stronger this force will be.")] public float CloseRepulsionForce;
        [Min(0f), Tooltip("The force applied to the AI when it is directly looking at the centre of an obstacle, making it swerve to the side to avoid crashing into it.")] public float AxisStalemateDeviationForce;
        [Min(0f), Tooltip("The size of the obstacle detection radius of the AI. The bigger the value, the further away of obstacles it can detect and try to avoid.")] public float ObstacleDetectRadius;
        [Min(0f), Tooltip("Should always be lower than ObstacleDetectRadius.")] public float CloseNavPointDetectionRadius;
        [Min(0f), Tooltip("The lerp speed for syncing the transform.forward of the AI to the rigidbody's velocity. This is only needed for the AI Graphics to function properly.")] public float SmoothLookAtSpeed;
        [Tooltip("Layers for which the AI should consider to be obstacles to avoid.")] public LayerMask ObstacleMask;
        [Tooltip("The physic material to apply to the AI associated with this steering setting.")] public PhysicMaterial PhysicMaterial;
		
		public event Action OnSettingsUpdate;
		public void UnsubscribeAllEvents()
		{
			OnSettingsUpdate = null;
		}
		
		private void OnValidate()
		{
			OnSettingsUpdate?.Invoke();
		}
    }
}
