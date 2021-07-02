using System.Collections.Generic;
using System.Linq;
using Hadal.Player;
using Tenshi;
using UnityEngine;

namespace Hadal.AI
{
    public class AIGameHandler : MonoBehaviour, ILeviathanComponent
    {
        [SerializeField, ReadOnly] private int _currentKillCount;
        private AIBrain _brain;
        private List<PlayerController> _players;

        public void DoLateUpdate(in float deltaTime) { }
        public void DoUpdate(in float deltaTime) { }
        public void DoFixedUpdate(in float fixedDeltaTime) { }

        public void Initialise(AIBrain brain)
        {
            _brain = brain;
            _players = brain.Players;
            _currentKillCount = 0;
            PlayerManager.Instance.OnAllPlayersReadyEvent += SetupEventListeners;
        }

        private void SetupEventListeners()
        {
            PlayerManager.Instance.OnAllPlayersReadyEvent -= SetupEventListeners;

            _brain.RefreshPlayerReferences();
            _players = _brain.Players.ToList();

            _players.ForEach(p => p.GetInfo.HealthManager.OnDeath += IncreaseKillCounter);
        }

        private void IncreaseKillCounter()
        {
            _currentKillCount++;
            EvaluatePlayerLoseGameEndState();
        }

        private void EvaluatePlayerLoseGameEndState()
        {
            _brain.RefreshPlayerReferences();
            if (_currentKillCount == 0)
            {

            }
        }

        public UpdateMode LeviathanUpdateMode => UpdateMode.LateUpdate;
    }
}
