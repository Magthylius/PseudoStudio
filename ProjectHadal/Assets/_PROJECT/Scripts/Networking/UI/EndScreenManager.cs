using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Hadal.Networking.UI.EndScreen
{
    public class EndScreenManager : MonoBehaviour
    {
        public static EndScreenManager Instance;

        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI missionOutcomeTMP;
        [SerializeField] private TextMeshProUGUI statusFlavourTMP;
        [SerializeField] private TextMeshProUGUI timeTakenTMP;
        [SerializeField] private TextMeshProUGUI timeTakenTitleTMP;

        [Header("Audio")]
        [SerializeField] private AudioSource endAudio;
        [SerializeField] private AudioClip winAudio;
        [SerializeField] private AudioClip loseAudio;

        [Header("Settings")] 
        [SerializeField, MinMaxSlider(1, 999)] private Vector2 squadNumberGenRange;
        [SerializeField, MinMaxSlider(1, 99)] private Vector2 leviathanNumberGenRange;
        [SerializeField] private string missionOutcomeText;
        [SerializeField] private string successOutcomeText;
        [SerializeField] private string failureOutcomeText;
        [SerializeField] private string statusFlavourSuccessText;
        [SerializeField] private string statusFlavourFailureText;
        [SerializeField] private Color successColor;
        [SerializeField] private Color failureColor;

        [Header("Data")]
        [ReadOnly, SerializeField] bool MissionSuccess = false;
        [ReadOnly, SerializeField] float TimeTaken = 0f;
        [ReadOnly, SerializeField] int GeneratedNumber = 0;
        private float currentTime = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        void Start()
        {
            //Debug.LogWarning("START IS CALLED?????");
            Disable(false);
        }

        public void Disable(bool restart)
        {
            //gameObject.SetActive(false);
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            StopCoroutine(UpdateTimeText());
            currentTime = 0f;

            if (restart) ApplicationHandler.RestartApp();
        }

        public void Enable()
        {
            //gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        public void UpdateEndData(bool gameWon, float timeTaken)
        {
            MissionSuccess = gameWon;
            TimeTaken = timeTaken;
            
            string outcomeText = missionOutcomeText;

            if (MissionSuccess)
            {
                string outComeCol = $"<color=#{ColorUtility.ToHtmlStringRGB(successColor)}>";
                outcomeText += $"{outComeCol}{successOutcomeText}";
                GeneratedNumber = Random.Range((int)leviathanNumberGenRange.x, (int)leviathanNumberGenRange.y);
                endAudio.clip = winAudio;

                statusFlavourTMP.text = $"{statusFlavourSuccessText}{string.Format(GeneratedNumber.ToString("D2"))}{outComeCol}";
            }
            else
            {
                string outComeCol = $"<color=#{ColorUtility.ToHtmlStringRGB(failureColor)}>";
                outcomeText += $"{outComeCol}{failureOutcomeText}";
                GeneratedNumber = Random.Range((int)squadNumberGenRange.x, (int)squadNumberGenRange.y);
                endAudio.clip = loseAudio;
                
                statusFlavourTMP.text = $"{statusFlavourFailureText}{string.Format(GeneratedNumber.ToString("D3"))}{outComeCol}";
            }

            statusFlavourTMP.text += $" HUNTED";
            
            endAudio.Play();

            TimeSpan timeSpan = TimeSpan.FromSeconds(TimeTaken);
            missionOutcomeTMP.text = outcomeText;
            //timeTakenTMP.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            timeTakenTMP.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

            //Debug.LogWarning(IsActive);
            //StartCoroutine(UpdateTimeText());
        }

        IEnumerator UpdateTimeText()
        {
            Debug.LogWarning(IsActive);
            while (TimeTaken - currentTime > 1f)
            {
                Debug.LogWarning(IsActive);
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

        public bool IsActive => gameObject.activeInHierarchy;
    }
}
