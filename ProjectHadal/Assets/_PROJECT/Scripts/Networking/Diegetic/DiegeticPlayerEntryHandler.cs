using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Hadal.Networking.Diegetics
{
    public class DiegeticPlayerEntryHandler : MonoBehaviour
    {
        [SerializeField] private List<Animator> otherPlayerAnimators;
        [SerializeField] private string entryTrigger;
        [SerializeField] private string exitTrigger;
        [SerializeField] private string boolState;

        //! Used as index, so first player enter -> 0
        private int totalPlayerCount = 0;

        /// <summary> Sets entry of already existing players.  </summary>
        /// <param name="playerCount">Player count of the room</param>
        public void UpdateCurrentEntered(int playerCount)
        {
            totalPlayerCount = playerCount;

            for (int i = 0; i < totalPlayerCount; i++)
            {
                if (i >= otherPlayerAnimators.Count)
                {
                    Debug.LogWarning($"Player count called more than animator count!");
                    totalPlayerCount = otherPlayerAnimators.Count - 1;
                    return;
                }
                
                otherPlayerAnimators[i].SetTrigger(entryTrigger);
                otherPlayerAnimators[i].SetBool(boolState, true);
            }
        }

        public void ExitAll()
        {
            totalPlayerCount = 0;
            foreach (var players in otherPlayerAnimators)
            {
                if (players.GetBool(boolState))
                    players.SetTrigger(exitTrigger);
            }
        }

        public void EnterOne()
        {
            totalPlayerCount++;
            
            if (totalPlayerCount >= otherPlayerAnimators.Count)
            {
                Debug.LogWarning($"Player count called more than animator count!");
                totalPlayerCount = otherPlayerAnimators.Count - 1;
                return;
            }
            
            otherPlayerAnimators[totalPlayerCount - 1].SetTrigger(entryTrigger);
            otherPlayerAnimators[totalPlayerCount - 1].SetBool(boolState, true);
        }

        public void ExitOne()
        {
            totalPlayerCount--;
            
            if (totalPlayerCount <= 0)
            {
                Debug.LogWarning($"Player count called less than animator count!");
                totalPlayerCount = 0;
                return;
            }
            
            otherPlayerAnimators[totalPlayerCount - 1].SetTrigger(exitTrigger);
            otherPlayerAnimators[totalPlayerCount - 1].SetBool(boolState, false);
        }

        [Button("Test entry")]
        void TestEntry() => UpdateCurrentEntered(1);
        
        [Button("Test exit")]
        void TestExit() => ExitAll();
    }

}