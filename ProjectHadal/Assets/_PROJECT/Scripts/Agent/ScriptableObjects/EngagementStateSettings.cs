using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Player;
using Tenshi;

namespace Hadal.AI.States
{
    /// <summary>
    /// Settings for Engagement state
    /// </summary>
    [CreateAssetMenu(fileName = "EngagementSettings", menuName = "StateSettings/Engagement")]
    public class EngagementStateSettings : StateSettings
    {
        [Header("General")]
        [Min(0f)] public float G_ApproachCloseDistanceThreshold;
        [Min(0f)] public float G_ApproachFarDistanceThreshold;
        [Min(0f)] public float G_CarryDelayTimer;
        [Min(0)] public int G_TotalThreshTimeInSeconds;
        [Min(0)] public int G_BaseThreshDamagePerSecond;
        [Min(0)] public int G_DisruptionDamageCount;

        [Header("Ambush Settings")]
        [Min(0f)] public float AM_TargetPlayerRange = 100f;
        [Min(0f)] public float AM_MaxWaitTime = 120f;
        [Min(0f)] public float AM_PounceSpeedMultiplier = 1.5f;
        [Min(0f)] public float AM_CarryDelayTimer;
        [Min(0)] public int AM_DisruptionDamageCount = 1;
        public int AM_AdditionalThreshDamagePerSecond;

        [Header("Hunt Settings")]
        [Min(0f)] public float HU_RoamingSpeedMultiplier = 1.5f;
        [Min(0f)] public float HU_MaxHuntingTime = 120f;
        [Min(0)] public int HU_DisruptionDamageCount = 1;

        [Header("Judgement Settings")]
        [Min(0f)] public float JudgementTimer1 = 30f;
        [Min(0f)] public float JudgementTimer2 = 45f;
        [Min(0f)] public float JudgementTimer3 = 60f;
        [Min(0f)] public float JudgementTimer4 = 90f;
        [Range(0f, 1f)] public float PostStunRemainJudgementChance = 0.8f;
        public int AGG_AdditionalThreshDamagePerSecond;
        public int DEF_AdditionalThreshDamagePerSecond;
        public int EGG_PermanentThreshDamagePerSecond;

        [Header("Unused ATM: Aggressive Settings")]
        [Range(0f, 1f)] public float AG_AccumulatedDamageThresholdPercentage = 0.4f;
        public bool AllowTarget_HighestDMGPlayer = true;
        public bool AllowTarget_HighestHPPlayer = true;
        public bool AllowTarget_IsolatedPlayer = true;

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
            return aiCurrentHealth * AG_AccumulatedDamageThresholdPercentage;
        }
        public float GetJudgementTimerThreshold(int index)
            => index switch
            {
                1 => JudgementTimer1,
                2 => JudgementTimer2,
                3 => JudgementTimer3,
                4 => JudgementTimer4,
                _ => 0f
            };
        
        public int GetThreshDamagePerSecond(EngagementType eType, bool eggDestroyed)
        {
            int additionalDPS = eType switch
            {
                EngagementType.Ambushing    => AM_AdditionalThreshDamagePerSecond,
                EngagementType.Defensive    => DEF_AdditionalThreshDamagePerSecond,
                EngagementType.Aggressive   => AGG_AdditionalThreshDamagePerSecond,
                _ => 0
            };
            int permanentDPS = eggDestroyed ? EGG_PermanentThreshDamagePerSecond : 0;

            return G_BaseThreshDamagePerSecond + additionalDPS + permanentDPS;
        }
    }

    public enum EngagementType
    {
        Ambushing,
        Defensive,
        Aggressive
    }
}