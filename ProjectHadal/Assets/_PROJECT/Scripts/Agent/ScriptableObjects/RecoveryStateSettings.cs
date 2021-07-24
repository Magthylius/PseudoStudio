using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.States
{
    /// <summary>
    /// Settings for Recovery state
    /// </summary>
    [CreateAssetMenu(fileName = "RecoverySettings", menuName = "StateSettings/Recovery")]
    public class RecoveryStateSettings : StateSettings
    {
        [Header("Recovery Settings")] 
        [Min(0f)] public float MinimumRecoveryTime;
        [Min(0)] public int G_DisruptionDamageCount = 2;
        [Min(0)] public int G_JudgementLapseCountLimit = 2;
        
        [Header("Escape Settings")]
        [Min(0f)] public float MaxEscapeTime = 100f;
        [Min(0f)] public float MaxEscapeDamageThresholdPercentage = 0.6f;
        [Min(0f)] public float EscapeSpeedMultiplier = 1.8f;

        public float GetEscapeDamageThreshold(float aiCurrentHealth)
        {
            return aiCurrentHealth * MaxEscapeDamageThresholdPercentage;
        }
    }
}
