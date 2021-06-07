using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.States
{
    /// <summary>
    /// Settings for Cooldown state
    /// </summary>
    [CreateAssetMenu(fileName = "CooldownSettings", menuName = "StateSettings/Cooldown")]
    public class CooldownStateSettings : StateSettings
    {
        [Header("Cooldown Settings")]
        [Min(0f)] public float MaxCooldownTime = 60f;
        [Min(0f)] public float ElusiveSpeedModifier = 2.5f;
    }
}
