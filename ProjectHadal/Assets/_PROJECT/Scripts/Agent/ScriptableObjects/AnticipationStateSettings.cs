using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.States
{
    /// <summary>
    /// Settings for Anticipation state
    /// </summary>
    [CreateAssetMenu(fileName = "AnticipationSettings", menuName = "StateSettings/Anticipation")]
    public class AnticipationStateSettings : StateSettings
    {
        [Header("Debug")]
        [Min(0f)] public float AutoActTime = 20f;

        [Header("General")]
        [Range(0f, 1f)] public float ConfidenceObjectiveGate = 0.5f;
        [MinMaxSlider(0f, 0.5f)] public Vector2 RandomConfidenceInfluence = Vector2.zero;
        [Min(0)] public int DisruptionDamageCount = 2;
        [Min(0)] public float IsolatedPlayerCheckTime = 5f;

        public EngagementObjective GetRandomInfluencedObjective(float currentConfidence)
        {
            if (currentConfidence + RandomInfluence < ConfidenceObjectiveGate)
                return EngagementObjective.Ambush;

            return EngagementObjective.Hunt;
        }

        public EngagementObjective GetClearObjective(float currentConfidence)
        {
            if (currentConfidence < ConfidenceObjectiveGate)
                return EngagementObjective.Ambush;

            return EngagementObjective.Hunt;
        }

        float RandomInfluence => Random.Range(-RandomConfidenceInfluence.y, RandomConfidenceInfluence.y);
    }
}
