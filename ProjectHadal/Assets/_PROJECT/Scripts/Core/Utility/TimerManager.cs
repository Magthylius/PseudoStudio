using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Utility
{
    public class TimerManager : MonoBehaviour
    {
        public static TimerManager Instance { get; private set; }
        private List<Timer> _timers = new List<Timer>();
        private List<Timer> _timersToAdd = new List<Timer>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        private void Update() => UpdateAllTimers();
        private void OnApplicationQuit() => StopAllTimers();

        public void RegisterTimer(Timer timer) => _timersToAdd.Add(timer);
        public void StopAllTimers()
        {
            int i = -1;
            while (++i < _timers.Count)
                _timers[i].Stop();

            _timers = new List<Timer>();
            _timersToAdd = new List<Timer>();
        }
        public void PauseAllTimers()
        {
            int i = -1;
            while (++i < _timers.Count)
                _timers[i].Pause();
        }
        public void ResumeAllTimers()
        {
            int i = -1;
            while (++i < _timers.Count)
                _timers[i].Resume();
        }
        public void ToggleAllTimers()
        {
            int i = -1;
            while (++i < _timers.Count)
                _timers[i].ToggleState();
        }
        public void RestartAllTimers()
        {
            int i = -1;
            while (++i < _timers.Count)
                _timers[i].Restart();
        }

        private void UpdateAllTimers()
        {
            HandleTimerAdditions();

            int i = -1;
            while (++i < _timers.Count)
                _timers[i].DoUpdate();

            PostUpdateTick();
        }

        private void HandleTimerAdditions()
        {
            if (_timersToAdd.IsEmpty()) return;
            _timers.AddRange(_timersToAdd);
            _timersToAdd.Clear();
        }

        private void PostUpdateTick()
        {
            for (int i = _timers.Count - 1; i >= 0; i--)
            {
                if (!_timers[i].IsDone) continue;
                _timers[i].Destroy();
                _timers.RemoveAt(i);
            }
        }
    }
}