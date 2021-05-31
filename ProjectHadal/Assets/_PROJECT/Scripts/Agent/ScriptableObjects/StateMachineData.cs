using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.States
{
    /// <summary>
    /// Houses a mode for all settings
    /// </summary>
    [CreateAssetMenu(fileName = "StateMachineData", menuName = "StateSettings/StateMachineData")]
    public class StateMachineData : ScriptableObject
    {
        [Header("State Settings reference")]
        public AnticipationStateSettings Anticipation;
        public EngagementStateSettings Engagement;
        public RecoveryStateSettings Recovery;
        public CooldownStateSettings Cooldown;

        [Header("Initial Settings")]
        [Min(0)] public int InitialConfidence;
        [Min(0)] public int NestDestroyedPermenantConfidence;
    }
}
