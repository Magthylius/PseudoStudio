using Hadal.AI.Caverns;
using Hadal.AI.States;
using UnityEngine;

namespace Hadal.AI
{
    public class JudgementBehaviourCoroutines
    {
        private AIBrain Brain;
        private PointNavigationHandler NavigationHandler;
        private LeviathanRuntimeData RuntimeData;
        private StateMachineData MachineData;
        private CavernManager CavernManager;
        private AIDamageManager DamageManager;
        private AIHealthManager HealthManager;
        private JudgementState JState;

        public JudgementBehaviourCoroutines(AIBrain brain, JudgementState judgementState)
        {
            Brain = brain;
            NavigationHandler = Brain.NavigationHandler;
            RuntimeData = Brain.RuntimeData;
            MachineData = Brain.MachineData;
            CavernManager = Brain.CavernManager;
            DamageManager = Brain.DamageManager;
            HealthManager = Brain.HealthManager;
            JState = judgementState;
        }

        #region Coroutines


        #endregion

        #region Utility & Verbose Shorthands




        #endregion
    }
}
