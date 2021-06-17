using System;
using System.Collections;
using System.Collections.Generic;
using Hadal.AI.Caverns;
using Hadal.AI.Information;
using NaughtyAttributes;
using UnityEngine;

namespace Hadal.AI.Information
{
    [RequireComponent(typeof(AIPackageInfo))]
    public class AIPackageDebugger : MonoBehaviour
    {
        private AIBrain brain;
        private PointNavigationHandler navHandler;
        
        [Header("States")] 
        [SerializeField] private BrainState overrideState;
        [SerializeField] private bool startWithOverrideState;
        
        private void Start()
        {
            brain = FindObjectOfType<AIBrain>();
            navHandler = brain.NavigationHandler;
            
            brain.SetOverrideState(overrideState);
            if (startWithOverrideState) brain.StartWithOverrideState();
        }

        [Header("Navigation Debug")]
        [SerializeField] private CavernTag targetCavern = CavernTag.Starting;
        [Button("Resume Logic")]
        void AIResumeLogic()
        {
            brain.SuspendState();
        }
        [Button("Force Go To Target Cavern")]
        void ForceGoToTargetCavern()
        {
            brain.ResumeState();

            brain.CavernManager.SeedCavernHeuristics(brain.CavernManager.GetCavern(targetCavern));
            CavernHandler nextCavern = brain.CavernManager.GetNextBestCavern(brain.CavernManager.GetHandlerOfAILocation);
 
            navHandler.SetImmediateDestinationToCavern(nextCavern);
        }
        
    }
}
