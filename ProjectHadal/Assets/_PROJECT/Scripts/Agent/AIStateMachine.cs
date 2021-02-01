using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class AIStateMachine : MonoBehaviour
{
    private Dictionary<Type, AIBaseState> availableStates;
    public AIBaseState currentState { get; private set; }

    public event Action<AIBaseState> OnStateChanged;

    void Start()
    {

    }

    void Update()
    {
        if(currentState == null)
        {
            currentState = availableStates.Values.First();
        }
                        //return own type or another type
        var nextState = currentState?.Tick();

                                //new state != currentState
        if(nextState != null && nextState != currentState?.GetType())
        {
            SwitchToNextState(nextState);
            Debug.LogWarning("My State: " + nextState);
        }
    }

    public void SetState(Dictionary<Type, AIBaseState> states)
    {
        availableStates = states;
    }

    void SwitchToNextState(Type nextState)
    {
        currentState = availableStates[nextState];
        OnStateChanged?.Invoke(currentState);
       
    }
}
