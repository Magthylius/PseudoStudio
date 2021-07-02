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
        [ReadOnly, SerializeField] bool MissionSuccess = false;
        [ReadOnly, SerializeField] float TimeTaken = 0f;
        private float currentTime = 0f;
        
        void Start()
        {
            Disable();
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            StopCoroutine(UpdateTimeText());
            currentTime = 0f;
        }

        public void Enable()
        {
            gameObject.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
 
        }

        public void UpdateEndData(bool gameWon, float timeTaken)
        {
          
            MissionSuccess = gameWon;
            TimeTaken = timeTaken;

            string outcomeText = missionOutcomeText;

            if (MissionSuccess)
                outcomeText += " <color=#" + ColorUtility.ToHtmlStringRGB(successColor) + "> " + successOutcomeText;
            else
                outcomeText += " <color=#" + ColorUtility.ToHtmlStringRGB(failureColor) + "> " + failureOutcomeText;

            TimeSpan timeSpan = TimeSpan.FromSeconds(TimeTaken);
            missionOutcomeTMP.text = outcomeText;
            //timeTakenTMP.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            timeTakenTMP.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

            StartCoroutine(UpdateTimeText());
        }

        IEnumerator UpdateTimeText()
        {
            
            while (TimeTaken - currentTime > 10f)
            {
                currentTime += 1f;
                TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
                timeTakenTMP.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                yield return null;
            }

            currentTime = TimeTaken;
            TimeSpan timeSpan2 = TimeSpan.FromSeconds(currentTime);
            timeTakenTMP.text = $"{timeSpan2.Hours:D2}:{timeSpan2.Minutes:D2}:{timeSpan2.Seconds:D2}";
            
            yield return null;
        }
    }
}
