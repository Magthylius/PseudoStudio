using System;
using System.Collections;
using System.Collections.Generic;
using Hadal.AI.Caverns;
using Hadal.AI.Information;
using NaughtyAttributes;
using UnityEngine;

namespace Hadal.AI
{
    [RequireComponent(typeof(AIPackageInfo))]
    public class AIPackageDebugger : MonoBehaviour
    {
        private AIBrain brain;
        private PointNavigationHandler navHandler;

        [SerializeField] private CavernTag targetCavern = CavernTag.Starting;
        
        private void Start()
        {
            brain = FindObjectOfType<AIBrain>();
            navHandler = brain.NavigationHandler;
        }

        [Button("Resume Logic")]
        void AIResumeLogic()
        {
            brain.SuspendStateLogic = false;
        }
        [Button("Force Go To Target Cavern")]
        void ForceGoToTargetCavern()
        {
            brain.SuspendStateLogic = true;
            navHandler.SetDestinationToCavern(brain.CavernManager, brain.CavernManager.GetCavern(targetCavern));
        }
    }
}
