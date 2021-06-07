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
        int timer;

        public ThreshCarriedPlayerNode(AIBrain brain, AIDamageManager damageManager)
        {
            _brain = brain;
            _damageManager = damageManager;
            _triggerOnce = false;
            _threshDone = false;
        }

        IEnumerator ThreshPlayer()
        {
            timer = _damageManager.ThreshTimer;

            while(timer > 0)
            {
                _damageManager.Send_DamagePlayer(_brain.CarriedPlayer.transform, AIDamageType.Thresh);
                yield return new WaitForSeconds(_damageManager.ApplyEveryNSeconds);
                timer--;
            }

            _threshDone = true;
        }

        public override NodeState Evaluate(float deltaTime)
        {
			if (_brain.CarriedPlayer == null)
                return NodeState.FAILURE;

            if(!_triggerOnce)
            {
                _triggerOnce = true;
                Debug.Log(_brain);
                _brain.StartCoroutine(ThreshPlayer());
            }

            if(_threshDone)
                return NodeState.SUCCESS;
            else
                return NodeState.RUNNING;

        }
    }
}
