using System;
using UnityEngine;

namespace Hadal.Utility
{
    public class TimerBuilder : IBuilder<Timer>
    {
        private Timer _timer;
        private MonoBehaviour _mono;
        public static TimerBuilder BuildATimer(MonoBehaviour mono) => new TimerBuilder(mono);
        private TimerBuilder(MonoBehaviour mono)
        {
            _mono = mono;
            _timer = new Timer().PausedOnStart();
        }
        public Timer Build() => _timer;

        public TimerBuilder WithDuration(in float duration)
        {
            _timer.Duration = duration;
            return this;
        }
        public TimerBuilder WithOnCompleteEvent(Action onComplete)
        {
            _timer.OnCompleteEvent = onComplete;
            return this;
        }
        public TimerBuilder WithOnUpdateEvent(Action<float> onUpdate)
        {
            _timer.OnUpdateEvent = onUpdate;
            return this;
        }
        public TimerBuilder WithLoop(bool loop)
        {
            _timer.Loop = loop;
            return this;
        }
        public TimerBuilder WithShouldPersist(bool shouldPersist)
        {
            _timer.ShouldPersist = shouldPersist;
            return this;
        }

        public static implicit operator Timer(TimerBuilder builder)
            => builder._mono.AttachTimer(builder.Build()).UnpausedOnStart();
    }
}