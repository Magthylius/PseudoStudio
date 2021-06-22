using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.AI.States;

namespace Hadal.AI.States
{
    /// <summary>
    /// Settings for Idle state
    /// </summary>
    [CreateAssetMenu(fileName = "IdleSettings", menuName = "StateSettings/Idle")]
    public class IdleStateSettings : StateSettings
    {
        [Min(0f)] public float StateExitDelay;
    }
}
