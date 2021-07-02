using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

namespace Hadal.Networking.UI.EndScreen
{
    public class EndScreenHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI missionOutcomeTMP;
        [SerializeField] private TextMeshProUGUI timeTakenTMP;

        [Header("Settings")] 
        [SerializeField] private string missionOutcomeText;
        [SerializeField] private string successOutcomeText;
        [SerializeField] private string failureOutcomeText;
        [SerializeField] private Color successColor;
        [SerializeField] private Color failureColor;
        
        [Header("Data")] 
        [ReadOnly] public bool MissionSuccess = false;
        [ReadOnly] public float TimeTaken = 0f;
        
        void Start()
        {
            Disable();
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void UpdateEndData(bool gameWon, float timeTaken)
        {
            MissionSuccess = gameWon;
            TimeTaken = timeTaken;

            string outcomeText = missionOutcomeText;

            if (MissionSuccess)
                outcomeText += " <color=" + successColor.ToString() + "> " + successOutcomeText;
            else
                outcomeText += " <color=" + failureColor.ToString() + "> " + failureOutcomeText;

            TimeSpan timeSpan = TimeSpan.FromSeconds(TimeTaken);
            missionOutcomeTMP.text = outcomeText;
            //timeTakenTMP.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            timeTakenTMP.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}
