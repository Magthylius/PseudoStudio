using UnityEngine;

namespace Hadal.AI
{
    [CreateAssetMenu(menuName = "AI/Boids Settings")]
    public class BoidsSettings : ScriptableObject
    {
        [Header("General")]
        [Range(0f, 20f)] public float ThrustSpeed;
        public float UpdateTimeInSeconds;

        [Header("Detection Distances")]
        [Range(0, 30)] public float CohesionDistance;
        [Range(0, 30)] public float SeparationDistance;
        [Range(0, 30)] public float AlignmentDistance;
        [Range(0, 30)] public float ObstacleDistance;

        [Header("Behaviour Weights")]
        [Range(0, 10)] public float CohesionWeight;
        [Range(0, 10)] public float SeparationWeight;
        [Range(0, 10)] public float AlignmentWeight;
        [Range(0, 10)] public float ObstacleWeight;
        [Range(0, 10)] public float AttractionWeight;
        
        [Header("Obstacle Detection")]
        public LayerMask ObstacleMask;
        [Range(1, 100)] public int ObstacleInfoBufferSize = 10;
    }
}