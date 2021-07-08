using Tenshi.AIDolls;
using System;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.Caverns;

namespace Hadal.AI.States
{
    public class AmbushState : AIStateBase
    {
        EngagementStateSettings settings;
        CavernHandler cavernHandler;
        CavernTag currentCavern;
        float ambushTimer;

        public AmbushState(AIBrain brain)
        {
            Initialize(brain);
            settings = MachineData.Engagement;
        }

        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
            currentCavern = Brain.CavernManager.GetCavernTagOfAILocation();
            cavernHandler = Brain.CavernManager.GetCavern(currentCavern);
            ambushTimer = settings.AM_MaxWaitTime;
        }
        public override void StateTick()
        {
            if (!NavigationHandler.Data_chosenAmbushPoint)
            {
                NavigationHandler.SelectAmbushPoint();
            }

            ambushTimer -= Brain.DeltaTime;
            $"AMBUSHTIMER: {ambushTimer}".Msg();

            if (cavernHandler.GetPlayerCount > 0)
            {
                RuntimeData.SetBrainState(BrainState.Judgement);
            }

            if(ambushTimer < 0)
            {
                RuntimeData.SetBrainState(BrainState.Anticipation);
            }
        }


        public override void LateStateTick() { }
        public override void FixedStateTick() { }
        public override void OnStateEnd() 
        { 
            NavigationHandler.ResetAmbushPoint();
        }
        public override Func<bool> ShouldTerminate() => () => false;

         
    }
}
