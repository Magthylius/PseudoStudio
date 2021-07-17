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

        private int currentEnteredCount = 0;

        /// <summary> Sets entry of already existing players.  </summary>
        /// <param name="count">Player count EXCLUDING the client!</param>
        public void UpdateCurrentEntered(int count)
        {
            currentEnteredCount = count;
            for (int i = 0; i < count; i++)
            {
                if (i >= otherPlayerAnimators.Count)
                {
                    Debug.LogWarning($"Player count called more than animator count!");
                    currentEnteredCount = otherPlayerAnimators.Count - 1;
                    return;
                }
                
                otherPlayerAnimators[i].SetTrigger(entryTrigger);
                otherPlayerAnimators[i].SetBool(boolState, true);
            }
        }

        public void ExitAll()
        {
            currentEnteredCount = 0;
            foreach (var players in otherPlayerAnimators)
            {
                if (players.GetBool(boolState))
                    players.SetTrigger(exitTrigger);
            }
        }

        public void EnterOne()
        {
            currentEnteredCount++;
            otherPlayerAnimators[currentEnteredCount].SetTrigger(entryTrigger);
            otherPlayerAnimators[currentEnteredCount].SetBool(boolState, true);
        }

        public void ExitOne()
        {
            currentEnteredCount--;
            otherPlayerAnimators[currentEnteredCount].SetTrigger(exitTrigger);
            otherPlayerAnimators[currentEnteredCount].SetBool(boolState, false);
        }

        [Button("Test entry")]
        void TestEntry() => UpdateCurrentEntered(1);
        
        [Button("Test exit")]
        void TestExit() => ExitAll();
    }

}