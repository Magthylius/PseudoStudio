using UnityEngine;
using NaughtyAttributes;

//Created by Jet
namespace Hadal.Locomotion
{
    [System.Serializable]
    public class SpeedInfo
    {
        [Header("Settings")]
        public float Max;
        public float InputForward;
        public float InputStrafe;
        public float InputHover;

        [Header("Reporting Stats")]
        [ReadOnly] public float Normalised;
        [ReadOnly] public float Forward;
        [ReadOnly] public float Strafe;
        [ReadOnly] public float Hover;

        public void Initialise()
        {

        }
    }

    [System.Serializable]
    public class AccelerationInfo
    {
        [Header("Settings")]
        public float Forward;
        public float Strafe;
        public float Hover;
        public float Boost;
        public float CummulationSpeed;
        public float MaxCummulation;

        [Header("Reporting Stats")]
        [ReadOnly] public float CummulatedAcceleration;

        public void Initialise()
        {
            CummulatedAcceleration = 0.0f;
        }
    }

    [System.Serializable]
    public class VelocityInfo
    {
        [Header("Reporting Stats")]
        [ReadOnly] public Vector3 Total;
        [ReadOnly] public float SquareSpeed;
        [ReadOnly] public float Speed;

        public void Initialise()
        {
            Total = Vector3.zero;
            SquareSpeed = 0.0f;
        }

        public void AddVelocity(Vector3 vector)
        {
            Total += vector;
        }
    }
}