using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Hadal.Networking;
using Hadal.Player;
using Photon.Pun;
using Tenshi;
using UnityEngine;

namespace Hadal.AI
{
    public class AIGameHandler : MonoBehaviour, ILeviathanComponent
    {
        [SerializeField, ReadOnly] private int _currentKillCount;
        private AIBrain _brain;

        /// <summary> Call this to end game and signify players win. </summary>
        public void AILoseGame()
        {
            GameManager.Instance.EndGameEvent(true);
        }

        /// <summary> Call this to end game and signify players lose. </summary>
        public void PlayersLoseGame()
        {
            GameManager.Instance.EndGameEvent(false);
        }

        public void DoLateUpdate(in float deltaTime) { }
        public void DoUpdate(in float deltaTime) { }
        public void DoFixedUpdate(in float fixedDeltaTime) { }

        public void Initialise(AIBrain brain)
        {
            _brain = brain;
            _currentKillCount = 0;

            //! Only setup listeners on master client
            if (PhotonNetwork.IsMasterClient)
                PlayerManager.Instance.OnAllPlayersReadyEvent += SetupEventListeners;
        }

        private void SetupEventListeners()
        {
            PlayerManager.Instance.OnAllPlayersReadyEvent -= SetupEventListeners;

            _brain.RefreshPlayerReferences();
            List<PlayerController> players = _brain.Players.ToList();

            players.ForEach(p => p.GetInfo.HealthManager.OnDeath += IncreaseKillCounter);
            players.ForEach(p => p.GetInfo.HealthManager.OnDown += CheckPlayersDown);
        }
        
        /// <summary> Increase counter per player death. </summary>
        private void IncreaseKillCounter()
        {
            _currentKillCount++;
            EvaluatePlayerLoseGameEndState();
        }

        /// <summary> Check if all players are down. </summary>
        void CheckPlayersDown()
        {
            List<PlayerController> players = _brain.Players.ToList();
            
            foreach (PlayerController p in players)
            {
                if (!p.GetInfo.HealthManager.IsDown) return;
            }

            PlayersLoseGame();
        }

        /// <summary> If all players in current room die, then end the game + send event. </summary>
        private void EvaluatePlayerLoseGameEndState()
        {
            _brain.RefreshPlayerReferences();
            int networkPlayerCount = NetworkEventManager.Instance.PlayerCount;
            
            bool killCountExceeded = _currentKillCount >= networkPlayerCount;
            bool leviathanIsNotDead = !_brain.HealthManager.IsUnalive;
            if (killCountExceeded && leviathanIsNotDead)
            {
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.PLAYER_ALL_UNALIVE, null, SendOptions.SendReliable);
                PlayersLoseGame();
            }
        }

        public UpdateMode LeviathanUpdateMode => UpdateMode.LateUpdate;
    }
}
