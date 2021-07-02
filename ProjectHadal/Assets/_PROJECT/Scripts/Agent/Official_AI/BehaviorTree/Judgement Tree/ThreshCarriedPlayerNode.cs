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
        float timer;
        float nextActionTime = 0;

        public ThreshCarriedPlayerNode(AIBrain brain, AIDamageManager damageManager)
        {
            _brain = brain;
            _damageManager = damageManager;
            _threshDone = false;
            _timerRunning = false;
            nextActionTime = 0f;
            _brain.RuntimeData.OnAIStateChange += ResetThreshNode;
        }

        ~ThreshCarriedPlayerNode()
        {
            _brain.RuntimeData.OnAIStateChange -= ResetThreshNode;
        }

        /// <summary>
        /// Resets only when switched to engagement + judgement state
        /// </summary>
        private void ResetThreshNode(BrainState bState, EngagementSubState eState)
        {
            if (bState == BrainState.Engagement && eState == EngagementSubState.Judgement)
            {
                timer = 0f;
                _timerRunning = false;
                nextActionTime = 0f;
                _threshDone = false;
            }
            else
            {
                //! Dont detach! Trust in the AI!
                //TryDetachCarriedPlayer();
            }
        }

        void StartTimer()
        {
            timer = _damageManager.ThreshTimer;
            Debug.LogWarning("Start timer:" + timer);
        }

        void ThreshPlayer()
        {
            Debug.LogWarning("Timer Thresh:" + timer);
            if (timer > 0f)
            {
                timer -= _brain.DeltaTime;
                // Debug.Log("Timer Thresh:" + timer);
                if (NextActionTimeReached())
                {
                    ResetNextActionTime();
                    _damageManager.Send_DamagePlayer(_brain.CarriedPlayer, AIDamageType.Thresh);
                    // Debug.Log("Damage:" + AIDamageType.Thresh);
                }

                bool NextActionTimeReached() => Time.time > nextActionTime;
                void ResetNextActionTime() => nextActionTime = Time.time + _damageManager.ApplyEveryNSeconds;
            }
            else
            {
                if (_timerRunning)
                {
                    timer = 0f;
                    _threshDone = true;
                }
            }
        }

        public override NodeState Evaluate(float deltaTime)
        {
            if (_brain.CarriedPlayer == null)
            {
                Debug.LogWarning("Carried player null");
                return NodeState.FAILURE;
            }
            Debugger();

            if (!_timerRunning)
            {
                _timerRunning = true;
                StartTimer();
            }

            ThreshPlayer();

            bool isDownOrIsUnalive = _brain.CarriedPlayer.GetInfo.HealthManager.IsDown || _brain.CarriedPlayer.GetInfo.HealthManager.IsUnalive;

            if (_threshDone || isDownOrIsUnalive)
            {
                Debug.LogWarning("thresh finished at " + _brain.NavigationHandler.Data_CurrentPoint.CavernTag);
                TryDetachCarriedPlayer();
                return NodeState.SUCCESS;
            }

            return NodeState.RUNNING;

        }

        private bool TryDetachCarriedPlayer()
        {
            if (_brain.CarriedPlayer == null)
                return false;

            _brain.CarriedPlayer.SetIsCarried(false);
            _brain.AttachCarriedPlayerToMouth(false);
            _brain.NavigationHandler.StopCustomPath(true);
            _brain.CarriedPlayer = null;
            return true;
        }

        public ThreshCarriedPlayerNode WithDebugName(string msg)
        {
            debugName = msg.AddSpacesBeforeCapitalLetters(false) + "?";
            return this;
        }

        private void Debugger()
        {
            if (EnableDebug)
                $"Name: {debugName}, Target: {_brain.CarriedPlayer.gameObject.name}".Msg();
        }
    }
}
