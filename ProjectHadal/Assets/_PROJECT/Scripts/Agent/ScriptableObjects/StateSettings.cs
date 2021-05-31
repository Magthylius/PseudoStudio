using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Player;

namespace Hadal.AI.States
{
    public enum AggressiveTargetMode
    {
        HighestDMG = 0,
        HighestHP,
        Isolated,
        TOTAL
    }

    /// <summary>
    /// Parent class for all state settings, should not have runtime-changable variables
    /// </summary>
    public class StateSettings : ScriptableObject
    {
        [Min(0f)] public float PlayerDetectRange = 100f;
        [Min(0)] public int ConfidenceIncrementValue = 10;
        [Min(0)] public int ConfidenceDecrementValue = 10;
    }
}
