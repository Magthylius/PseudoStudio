using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tenshi.AIDolls
{
    public class StateMachine
    {
        private readonly Dictionary<Type, List<Transition>> AllTransitions = new Dictionary<Type, List<Transition>>();
        private readonly List<Transition> EventTransitions = new List<Transition>();
        private List<Transition> SequentialTransitions = new List<Transition>();

        private IState _currState;
        public IState CurrentState => _currState;

        /// <summary> Should be called in an update function. It is this state machine's update method. </summary>
        public void MachineTick()
        {
            var transition = GetActiveTransition();
            if (transition != null) SetState(transition.Target);
            _currState?.StateTick();
        }

        /// <summary> Used to force a state change. Aside from setting a default state, try not to use this for anything else. </summary>
        public bool SetState(IState newState)
        {
            if (newState == _currState) return false;
            _currState?.OnStateEnd();
            _currState = newState;
            AllTransitions.TryGetValue(_currState.GetType(), out SequentialTransitions);
            if (SequentialTransitions.IsNullOrEmpty()) SequentialTransitions = Transition.Null;
            _currState?.OnStateStart();
            return true;
        }

        /// <summary> Used to add a sequential transition. A sequential transition must have a <paramref name="from"/> state
        /// to transition to the <paramref name="to"/> state. This transition will automatically happen when the passed in
        /// <paramref name="withCondition"/> evaluates to true at any point in the program. </summary>
        public void AddSequentialTransition(IState from, IState to, Func<bool> withCondition)
        {
            if (!AllTransitions.TryGetValue(from.GetType(), out var transitions))
            {
                transitions = new List<Transition>();
                AllTransitions[from.GetType()] = transitions;
            }
            transitions.Add(new Transition(to, withCondition));
        }

        /// <summary> Used to add an event transition. An event transition only has a <paramref name="to"/> state as it can
        /// transition from any other state in the machine (something like a 'drop everything and do that' transition). This
        /// transition will automatically happen when the passed in <paramref name="withCondition"/> evaluates to true at any 
        /// point in the program. </summary>
        public void AddEventTransition(IState to, Func<bool> withCondition) => EventTransitions.Add(new Transition(to, withCondition));

        private Transition GetActiveTransition()
        {
            var activeEvent = FirstActiveOf(EventTransitions);
            if (activeEvent != null) return activeEvent;

            var activeSequence = FirstActiveOf(SequentialTransitions);
            if (activeSequence != null) return activeSequence;

            return null;

            #region Shorthand
            static Transition FirstActiveOf(List<Transition> transitions) => transitions.Where(t => t.Condition()).FirstOrDefault();
            #endregion
        }

        private class Transition
        {
            /// <summary> If <paramref name="Condition"/> evaluates to true, a transition would occur into the <paramref name="Target"/> state. </summary>
            public Func<bool> Condition { get; }
            /// <summary> <see cref="Condition"/> </summary>
            public IState Target { get; }
            
            public static List<Transition> Null => new List<Transition>(0);
            public Transition(IState target, Func<bool> condition)
            {
                Target = target;
                Condition = condition;
            }
        }
    }

    public class ArtificialBehaviourState : IState
    {
        private readonly ArtificialBehaviour _behaviour = null;
        public ArtificialBehaviourState(ArtificialBehaviour behaviour) => _behaviour = behaviour;
        public void OnStateStart() => _behaviour.OnStart();
        public void StateTick() => _behaviour.OnUpdate();
        public void OnStateEnd() => _behaviour.OnEnd();
        public Func<bool> ShouldTerminate() => () => false;
    }

    public abstract class ArtificialBehaviour : MonoBehaviour
    {
        public abstract void OnStart();
        public abstract void OnUpdate();
        public abstract void OnEnd();
    }

    public interface IState
    {
        /// <summary> This is the Update method of the state. If it is the active state, it will be called by <see cref="StateMachine.MachineTick"/>. </summary>
        void StateTick();
        
        /// <summary> This will be called when the state becomes the active state (transitioned into from another state). </summary>
        void OnStateStart();
        
        /// <summary> This will be called when the state becomes inactive (transitioned out to another state). </summary>
        void OnStateEnd();

        /// <summary> Can be used to specify a 'self termination' condition if this state needs to end itself at some point. 
        /// This is so that individual state can tell the machine that they should terminate without the machine needing to
        /// knowing the details. </summary>
        Func<bool> ShouldTerminate();
    }
}