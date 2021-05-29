using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.P

namespace Hadal.AI
{
    public class StateSettings : ScriptableObject
    {

    }

    [CreateAssetMenu(fileName = "AnticipationSettings", menuName = "StateSettings/Anticipation")]
    public class AnticipationStateSettings : StateSettings
    {
        [Range(0f, 1f)] public float ConfidenceObjectiveGate = 0.5f;
        [MinMaxSlider(0f, 0.5f)] public Vector2 RandomConfidenceInfluence = Vector2.zero;

        public Objective GetRandomInfluencedObjective(float currentConfidence)
        {
            if (currentConfidence + RandomInfluence < ConfidenceObjectiveGate)
                return Objective.Ambush;

           return Objective.Aggressive;
        }

        public Objective GetClearObjective(float currentConfidence)
        {
            if (currentConfidence < ConfidenceObjectiveGate)
                return Objective.Ambush;

            return Objective.Aggressive;
        }

        float RandomInfluence => Random.Range(-RandomConfidenceInfluence.y, RandomConfidenceInfluence.y);
    }

    [CreateAssetMenu(fileName = "EngagementSettings", menuName = "StateSettings/Engagement")]
    public class EngagementStateSettings : StateSettings
    {
        [Header("Ambush Settings")]
        [Min(0f)] public float AmbushTargetPlayerRange = 100f;

        [Header("Aggressive Settings")]
        [Min(0f)] public float AggressiveTargetPlayerRange = 100f;
        public bool AllowTarget_HighestDMGDealer = true;
        public bool AllowTarget_HighestHPPlayer = true;
        public bool AllowTarget_IsolatedPlayer = true;

        //public Player

    }
}
