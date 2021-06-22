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
        private float gameTime = 0f;

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

        private void FixedUpdate()
        {
            if (!allowTimerTick) return;
            gameTime += Time.fixedDeltaTime;
        }

        IEnumerator TimerNumeration()
        {
            while (allowTimerTick)
            {
                var ts = TimeSpan.FromSeconds(gameTime);
                timerTMP.text = $"{ts.Minutes:00}:{ts.Seconds:00}";
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
