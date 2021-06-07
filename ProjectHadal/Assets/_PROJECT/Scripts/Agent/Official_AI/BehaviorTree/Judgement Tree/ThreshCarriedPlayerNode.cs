using Tenshi;
using Tenshi.UnitySoku;
using UnityEngine;
using System.Collections;

namespace Hadal.AI.TreeNodes
{
    public class ThreshCarriedPlayerNode : BTNode
    {
        private AIBrain _brain;
        AIDamageManager _damageManager;
        bool _threshDone;
        bool _triggerOnce;


        public ThreshCarriedPlayerNode(AIBrain brain, AIDamageManager damageManager)
        {
            _brain = brain;
            _damageManager = damageManager;
            _triggerOnce = false;
            _threshDone = false;
        }

        void ThreshPlayer()
        {
            for (int i = 0; i < _damageManager.threshTimer; i--)
            {
                _damageManager.Send_DamagePlayer(_brain.CarriedPlayer.transform, AIDamageType.Thresh);
                if (_damageManager.threshTimer <= 0)
                    _threshDone = true;
            }

        }

        public override NodeState Evaluate()
        {
			if (_brain.CarriedPlayer == null)
                return NodeState.FAILURE;

            if(!_triggerOnce)
            {
                _triggerOnce = true;
                ThreshPlayer();
            }
				
            // if (_brain.CarriedPlayer.GetInfo.HealthManager.)
            while(!_threshDone)
                return NodeState.RUNNING;

            "AI: I am hurting the player".Bold().Msg();
            return NodeState.RUNNING;
        }
    }
}
