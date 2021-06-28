using Tenshi.AIDolls;
using System;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.Caverns;

namespace Hadal.AI.States
{
    public class AmbushSubState : AIStateBase
    {
        EngagementState parent;
        AIBrain b;
        EngagementStateSettings engagementStateSettings;
        PointNavigationHandler navigationHandler;
        CavernHandler cavernHandler;
        CavernTag currentCavern;
        float ambushTimer;

        public AmbushSubState()
        {

        }
        public void Initialize(EngagementState parent)
        {
            this.parent = parent;
            Initialize(parent.Brain);
            Brain = parent.Brain;
            navigationHandler = Brain.NavigationHandler;
            engagementStateSettings = MachineData.Engagement;
        }

        public override void OnStateStart()
        {
            if (b.DebugEnabled) $"Switch substate to: {this.NameOfClass()}".Msg();
            currentCavern = Brain.CavernManager.GetCavernTagOfAILocation();
            cavernHandler = Brain.CavernManager.GetCavern(currentCavern);
            ambushTimer = engagementStateSettings.AM_MaxWaitTime;
        }
        public override void StateTick()
        {
            if (!navigationHandler.Data_chosenAmbushPoint)
            {
                navigationHandler.SelectAmbushPoint();
            }

            ambushTimer -= Brain.DeltaTime;

            if (cavernHandler.GetPlayerCount > 0)
            {
                RuntimeData.SetEngagementSubState(EngagementSubState.Judgement);
            }
            else if(ambushTimer <= 0)
            {
                RuntimeData.SetBrainState(BrainState.Anticipation);
            }
        }


        public override void LateStateTick() { }
        public override void FixedStateTick() { }
        public override void OnStateEnd() { }
        public override Func<bool> ShouldTerminate() => () => false;
    }
}
