using Tenshi.AIDolls;
using System;
using Tenshi;
using Tenshi.UnitySoku;

namespace Hadal.AI.States
{
    public class AmbushSubState : AIStateBase
    {
        EngagementState parent;
        AIBrain b;

        public AmbushSubState()
        {

        }
        public void SetParent(EngagementState parent)
        {
            this.parent = parent;
            b = parent.Brain;
        }

        public override void OnStateStart()
		{
			if (b.DebugEnabled) $"Switch substate to: {this.NameOfClass()}".Msg();
		}
        public override void StateTick()
        {
            //! new logic
            /*
            if (has not chosen ambush point)
            {
                find ambush location { closest to AI? closest to Players? }
            }

            if (has chosen ambush point)
            {
                path towards ambush point;
                if (player interruption)
                {
                    confidence--
                    goto -> Judgement substate and evaluate normally
                }
            }

            if (is in ambush point)
            {
                if (any player is within bite range)
                {
                    try to bite that player;
                    goto -> Judgement substate and evaluate normally
                }
                else
                {
                    tick wait timer;
                    if (wait timer exceeded && no players in current cavern)
                    {
                        confidence++
                        goto -> Anticipation state
                    }
                }
            }
            */
        }
        public override void LateStateTick() { }
        public override void FixedStateTick() { }
        public override void OnStateEnd() { }
        public override Func<bool> ShouldTerminate() => () => false;
    }
}
