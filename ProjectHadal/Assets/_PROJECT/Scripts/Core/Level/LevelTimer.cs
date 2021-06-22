using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Hadal
{
    public class LevelTimer : MonoBehaviour
    {
        private TextMeshProUGUI timerTMP;
        private bool allowTimerTick = false;

        void Start()
        {
            timerTMP = GetComponent<TextMeshProUGUI>();
            GameManager.Instance.GameStartedEvent += StartTimer;
            GameManager.Instance.GameEndedEvent += StopTimer;
        }

        void StartTimer()
        {
            allowTimerTick = true;
            StartCoroutine(TimerNumeration());
        }

        void StopTimer()
        {
            allowTimerTick = false;
            StopCoroutine(TimerNumeration());
        }

        IEnumerator TimerNumeration()
        {
            while (allowTimerTick)
            {
                timerTMP.text = TimeSpan.FromSeconds(Time.timeSinceLevelLoad).ToString("mm:ss");
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
