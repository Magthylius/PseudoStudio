using Tenshi;
using Tenshi.UnitySoku;
using System;
using Hadal.AI.Caverns;

namespace Hadal.AI.States
{
    public class EngagementState : AIStateBase
    {
        private EngagementStateSettings settings;

        public EngagementState(AIBrain brain)
        {
            Initialize(brain);
            settings = brain.MachineData.Engagement;
        }
        
        public override void OnStateStart()
        {
            if (Brain.DebugEnabled) $"Switch state to: {this.NameOfClass()}".Msg();
            
        }
        public override void StateTick()
        {
            if (!AllowStateTick) return;
            
        }
        public override void LateStateTick()
        {
            if (!AllowStateTick) return;

        }
        public override void FixedStateTick()
        {
            if (!AllowStateTick) return;

        }
        public override void OnStateEnd()
        {
            
        }

        public override void OnCavernEnter(CavernHandler cavern)
        {
            Brain.UpdateTargetMoveCavern(AICavern);
        }

        public override Func<bool> ShouldTerminate() => () => false;
    }
}
