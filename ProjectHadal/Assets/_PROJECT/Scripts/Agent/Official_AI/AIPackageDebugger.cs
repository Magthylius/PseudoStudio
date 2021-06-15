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

            //print(cavernManager.GetHandlerOfAILocation);
            //print(brain.CavernManager.GetCavern(targetCavern));
            brain.CavernManager.SeedCavernHeuristics(brain.CavernManager.GetHandlerOfAILocation, brain.CavernManager.GetCavern(targetCavern));
            CavernHandler nextCavern = brain.CavernManager.GetNextBestCavern(brain.CavernManager.GetHandlerOfAILocation);
            //brain.SuspendStateLogic = true;
            navHandler.SetDestinationToCavern(brain.CavernManager, nextCavern);
        }
    }
}
