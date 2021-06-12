using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
{
    public class AISteeringSettings : ScriptableObject
    {
        [Header("Steering Settings")]
        public float MaxVelocity;
        public float ThrustForce;
        public float AdditionalBoostThrustForce;
        public float AttractionForce;
        public float AvoidanceForce;
        public float CloseRepulsionForce;
        public float AxisStalemateDeviationForce;
        public float ObstacleDetectRadius;
        public float SmoothLookAtSpeed;

        [Header("Transition Settings")] 
        [Range(0f, 1f)] public float transitionTime;
    }
}
