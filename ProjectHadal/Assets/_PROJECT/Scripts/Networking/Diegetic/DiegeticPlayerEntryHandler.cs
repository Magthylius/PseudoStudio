using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Networking.Diegetics
{
    public class DiegeticPlayerEntryHandler : MonoBehaviour
    {
        [SerializeField] private List<Animator> otherPlayerAnimators;
        [SerializeField] private string entryTrigger;
        [SerializeField] private string exitTrigger;

        private int currentEnteredCount = 0;

        public void UpdateCurrentEntered(int count)
        {
            currentEnteredCount = count;
            for (int i = 0; i < count; i++)
            {
                otherPlayerAnimators[i].SetTrigger(entryTrigger);
            }
        }

        public void ExitAll()
        {
            currentEnteredCount = 0;
            foreach (var players in otherPlayerAnimators)
            {
                players.SetTrigger(exitTrigger);
            }
        }

        public void EnterOne()
        {
            currentEnteredCount++;
            otherPlayerAnimators[currentEnteredCount].SetTrigger(entryTrigger);
        }

        public void ExitOne()
        {
            currentEnteredCount--;
            otherPlayerAnimators[currentEnteredCount].SetTrigger(exitTrigger);
        }
    }

}