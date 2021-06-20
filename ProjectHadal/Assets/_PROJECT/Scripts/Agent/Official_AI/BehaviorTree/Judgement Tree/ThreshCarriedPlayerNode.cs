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
        bool _timerRunning;
        bool _doOnce;
        float timer;
        float nextActionTime = 0;

        public ThreshCarriedPlayerNode(AIBrain brain, AIDamageManager damageManager)
        {
            _brain = brain;
            _damageManager = damageManager;
            _threshDone = false;
            _timerRunning = false;
            _doOnce = false;
        }

        void StartTimer()
        {
            timer = _damageManager.ThreshTimer;
        }

        void ThreshPlayer()
        {
            if (timer > 0)
            {
                timer = timer - 1;
                Debug.Log("Timer Thresh:" + timer);
                if (Time.time > nextActionTime)
                {
                    nextActionTime = Time.time + _damageManager.ApplyEveryNSeconds;
                    _damageManager.Send_DamagePlayer(_brain.CarriedPlayer.transform, AIDamageType.Thresh);
                }

            }
            else
            {
                timer = 0;
                _threshDone = true;
            }
        }

        public override NodeState Evaluate(float deltaTime)
        {
            Debugger();
            if (_brain.CarriedPlayer == null)
                return NodeState.FAILURE;

            if (!_doOnce)
            {
                _doOnce = true;
                StartTimer();
            }

            ThreshPlayer();

            if (_threshDone)
            {
                Debug.LogWarning("thresh finished " + _brain.NavigationHandler.Data_CurrentPoint.CavernTag);
                _brain.CarriedPlayer.SetIsCarried(false);
                _brain.CarriedPlayer = null;
                _brain.AttachCarriedPlayerToMouth(false);
                _brain.NavigationHandler.StopCustomPath(false);
                return NodeState.SUCCESS;
            }

            return NodeState.RUNNING;

        }

         public ThreshCarriedPlayerNode WithDebugName(string msg)
        {
            debugName = msg.AddSpacesBeforeCapitalLetters(false) + "?";
            return this;
        }

        private void Debugger()
        {
            if (EnableDebug)
                $"Name: {debugName}, Target: {_brain.CarriedPlayer}".Msg();
        }
    }
}
