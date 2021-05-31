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
}
