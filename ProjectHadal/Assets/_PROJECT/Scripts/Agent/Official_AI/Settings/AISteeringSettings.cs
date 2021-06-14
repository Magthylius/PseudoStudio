using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.Settings
{
    [CreateAssetMenu(fileName = "New Steering Settings", menuName = "AI/Steering Settings")]
    public class AISteeringSettings : ScriptableObject
    {
        [Header("Steering Settings")]
        [Min(0f)] public float MaxVelocity;
        [Min(0f)] public float ThrustForce;
        [Min(0f)] public float AdditionalAttractionForce;
        [Min(0f)] public float AttractionForce;
        [Min(0f)] public float AvoidanceForce;
        [Min(0f)] public float CloseRepulsionForce;
        [Min(0f)] public float AxisStalemateDeviationForce;
        [Min(0f)] public float ObstacleDetectRadius;
        [Min(0f), Tooltip("Should always be lower than ObstacleDetectRadius.")] public float CloseNavPointDetectionRadius;
        [Min(0f)] public float SmoothLookAtSpeed;
        public LayerMask ObstacleMask;
    }
}
