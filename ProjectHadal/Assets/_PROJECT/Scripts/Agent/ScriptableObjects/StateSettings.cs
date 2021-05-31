using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Player;

namespace Hadal.AI
{
    public enum AggressiveTargetMode
    {
        HighestDMG = 0,
        HighestHP,
        Isolated,
        TOTAL
    }

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
        [Min(0f)] public float AM_TargetPlayerRange = 100f;
        [Min(0f)] public float AM_MaxWaitTime = 120f;

        [Header("Aggressive Settings")]
        [Min(0f)] public float AG_TargetPlayerRange = 100f;
        [Range(0f, 1f)] public float AG_AccumulatedDamageThresholdPercentage = 0.4f;
        public bool AllowTarget_HighestDMGPlayer = true;
        public bool AllowTarget_HighestHPPlayer = true;
        public bool AllowTarget_IsolatedPlayer = true;

        [Header("Judgement Settings")]
        [Min(0f)] public float JudgementTimer1 = 30f;
        [Min(0f)] public float JudgementTimer2 = 45f;
        [Min(0f)] public float JudgementTimer3 = 60f;
        [Min(0f)] public float JudgementTimer4 = 90f;

        public PlayerController AM_GetRandomAmbushPoint()
        {
            //! TODO: Cavern manager
            return null;
        }
        public PlayerController AG_GetRandomTargetPlayer()
        {
            PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
            PlayerController targetPlayer = allPlayers[Random.Range(0, allPlayers.Length)];

            bool loopExit = true;
            int loopFailSafe = 0;
            do
            {
                AggressiveTargetMode mode = (AggressiveTargetMode)Random.Range((int)AggressiveTargetMode.HighestDMG, (int)AggressiveTargetMode.TOTAL);
                bool cond1 = !AllowTarget_HighestDMGPlayer && mode == AggressiveTargetMode.HighestDMG;
                bool cond2 = !AllowTarget_HighestHPPlayer && mode == AggressiveTargetMode.HighestHP;
                bool cond3 = !AllowTarget_IsolatedPlayer && mode == AggressiveTargetMode.Isolated;

                if (cond1 || cond2 || cond3) 
                {
                    loopExit = false;
                    loopFailSafe++;
                }
                else
                {
                    int highestHp = int.MaxValue;

                    switch (mode)
                    {
                        case AggressiveTargetMode.HighestDMG:
                            //! TODO: Need player stats
                            loopExit = false;
                            loopFailSafe++;
                            break;
                        case AggressiveTargetMode.HighestHP:  
                            foreach (PlayerController player in allPlayers)
                            {
                                if (player.GetInfo.HealthManager.GetCurrentHealth < highestHp)
                                {
                                    highestHp = player.GetInfo.HealthManager.GetCurrentHealth;
                                    targetPlayer = player;
                                }
                            }
                            break;
                        case AggressiveTargetMode.Isolated:
                            //! TODO: need cavern manager
                            loopExit = false;
                            loopFailSafe++;
                            break;
                        default:
                            break;
                    }
                }

                if (loopFailSafe > 100)
                {
                    Debug.LogError("AI aggro target loop exceeded 100!, defaulted to random player.");
                    loopExit = true;
                }

            } while (!loopExit);

            return targetPlayer;
        }
        public float GetAccumulatedDamageThreshold(float aiCurrentHealth)
        {
            return AG_AccumulatedDamageThresholdPercentage * aiCurrentHealth;
        }
    }

}
