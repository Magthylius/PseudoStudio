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
        [Range(0f, 1f)] public float ConfidenceObjectiveGate = 0.5f;
        [MinMaxSlider(0f, 0.5f)] public Vector2 RandomConfidenceInfluence = Vector2.zero;

        public EngagementSubState GetRandomInfluencedObjective(float currentConfidence)
        {
            if (currentConfidence + RandomInfluence < ConfidenceObjectiveGate)
                return EngagementSubState.Ambush;

            return EngagementSubState.Aggressive;
        }

        public EngagementSubState GetClearObjective(float currentConfidence)
        {
            if (currentConfidence < ConfidenceObjectiveGate)
                return EngagementSubState.Ambush;

            return EngagementSubState.Aggressive;
        }

        float RandomInfluence => Random.Range(-RandomConfidenceInfluence.y, RandomConfidenceInfluence.y);
    }
}
