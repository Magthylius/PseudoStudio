using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.States
{
    /// <summary>
    /// Settings for Cooldown state
    /// </summary>
    [CreateAssetMenu(fileName = "LureSettings", menuName = "StateSettings/Lure")]
    public class LureStateSettings : StateSettings
    {
        [Header("Lure Settings")]
        [Min(0f)] public float StateExitDelay;
    }
}
