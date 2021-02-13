using System;
using System.Reflection;
using UnityEngine;

namespace Hadal.Utility
{
    public class Timer
    {
        #region Variable Definitions

        #region Builder Components
        public Action OnCompleteEvent { get; internal set; } = null;
        public Action<float> OnUpdateEvent { get; internal set; } = null;
        public bool Loop { get; internal set; } = false;
        public float Duration { get; internal set; } = 0.0f;
        public bool ShouldPersist { get; internal set; } = false;
        #endregion

        public bool IsCompleted { get; private set; }
        public bool IsPaused => _timeElapsedBeforePause.HasValue;
        public bool IsStopped => _timeElapsedBeforeStop.HasValue;
        public bool IsDone => (IsCompleted || IsStopped || IsOwnerDestroyed) && !ShouldPersist;
        public float GetRemainingTime => Duration - GetElapsedTime();
        public float GetCompletionRatio => GetElapsedTime() / Duration;
        public float GetRemainingRatio => GetRemainingTime / Duration;
        private float GetTime => Time.time;
        private float GetTriggerTime => _startTime + Duration;
        private float GetLocalDeltaTime => GetTime - _lastUpdateTime;

        private static TimerManager _manager;
        private float _startTime;
        private float _lastUpdateTime;
        private float? _timeElapsedBeforeStop = null;
        private float? _timeElapsedBeforePause = null;
        private float _startDuration = 0.0f;
        private bool _completedOnStart = false;
        private MonoBehaviour _monoOwner;
        private readonly bool _monoDependant;
        private bool IsOwnerDestroyed => _monoDependant && _monoOwner == null;

        #endregion

        #region Timer Methods

        public void Stop()
        {
            if (IsDone) return;
            _timeElapsedBeforeStop = GetElapsedTime();
            _timeElapsedBeforePause = null;
            if (ShouldPersist) ShouldPersist = false;
        }

        public void Pause()
        {
            if (IsPaused || IsDone) return;
            _timeElapsedBeforePause = GetElapsedTime();
        }

        public void Resume()
        {
            if (!IsPaused || IsDone) return;
            _timeElapsedBeforePause = null;
        }

        public void ToggleState()
        {
            if (IsDone) return;
            if (IsPaused)
            {
                Resume();
                return;
            }
            Pause();
        }

        public void Restart()
        {
            IsCompleted = false;
            Resume();
            _startTime = GetTime;
            _lastUpdateTime = _startTime;
        }

        public float GetElapsedTime()
        {
            if (IsCompleted || GetTime.IsGreaterOrEqualTo(GetTriggerTime))
                return Duration;

            return _timeElapsedBeforeStop ?? _timeElapsedBeforePause ?? DefaultElapsed;
        }

        internal void DoUpdate()
        {
            if (IsDone || HandlePause()) return;
            UpdateTick();
            if (GetTime.IsGreaterOrEqualTo(GetTriggerTime)) HandleTimerCompleteEvent();
            PostUpdateTick();
        }

        private bool HandlePause()
        {
            if (!IsPaused) return false;
            _startTime += GetLocalDeltaTime;
            _lastUpdateTime = GetTime;
            return true;
        }

        private void UpdateTick()
        {
            _lastUpdateTime = GetTime;
            if (_completedOnStart) return;
            OnUpdateEvent?.Invoke(GetElapsedTime());
        }

        private void HandleTimerCompleteEvent()
        {
            if (IsCompleted) return;

            if (Loop)
                _startTime = GetTime;
            else
                IsCompleted = true;
            
            if (_completedOnStart) return;
            OnCompleteEvent?.Invoke();
        }

        private void PostUpdateTick()
        {
            if (!_completedOnStart) return;
            _completedOnStart = false;
            Duration = _startDuration;
            _startDuration = 0.0f;
        }

        private float DefaultElapsed => GetTime - _startTime;

        #endregion

        #region Constructor Builder

        internal Timer() { }

        private Timer(float duration, Action onComplete, Action<float> onUpdate, bool loop, MonoBehaviour monoOwner, bool shouldPersist)
        {
            Duration = duration;
            OnCompleteEvent = onComplete;
            OnUpdateEvent = onUpdate;
            Loop = loop;
            _monoOwner = monoOwner;
            _monoDependant = monoOwner != null;
            ShouldPersist = shouldPersist;
            Restart();
        }

        public Timer PausedOnStart()
        {
            Pause();
            return this;
        }

        public Timer UnpausedOnStart()
        {
            Resume();
            return this;
        }

        public Timer CompletedOnStart()
        {
            _startDuration = Duration;
            Duration = 0.0f;
            _completedOnStart = true;
            return this;
        }

        #endregion

        #region Static Utility

        public static Timer Register(Timer timer)
        {
            HandleTimerManager();
            _manager.RegisterTimer(timer);
            return timer;
        }

        public static Timer Register(float duration, Action onComplete, Action<float> onUpdate = null,
            bool loop = false, MonoBehaviour monoOwner = null, bool shouldPersist = false)
        {
            HandleTimerManager();
            Timer timer = new Timer(duration, onComplete, onUpdate, loop, monoOwner, shouldPersist);
            _manager.RegisterTimer(timer);
            return timer;
        }

        public static void StopAllRegisteredTimers()
        {
            if (_manager == null) return;
            _manager.StopAllTimers();
        }

        public static void PauseAllRegisteredTimers()
        {
            if (_manager == null) return;
            _manager.PauseAllTimers();
        }

        public static void ResumeAllRegisteredTimers()
        {
            if (_manager == null) return;
            _manager.ResumeAllTimers();
        }

        public static void ToggleAllRegisteredTimers()
        {
            if (_manager == null) return;
            _manager.ToggleAllTimers();
        }

        public static void RestartAllRegisteredTimers()
        {
            if (_manager == null) return;
            _manager.RestartAllTimers();
        }

        private static void HandleTimerManager()
        {
            if (_manager != null) return;
            if (TimerManager.Instance != null)
            {
                _manager = TimerManager.Instance;
                return;
            }
            GameObject managerObj = new GameObject("TimerManager");
            _manager = managerObj.AddComponent<TimerManager>();
        }

        #endregion
    }
}