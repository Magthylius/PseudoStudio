using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Photon.Pun;

public class AIStateMachine : MonoBehaviour
{
    private Dictionary<Type, AIBaseState> availableStates;
    public AIBaseState currentState { get; private set; }

    public event Action<AIBaseState> OnStateChanged;

    [SerializeField] PhotonTransformViewClassic PV;

    void Start()
    {
        PV = gameObject.GetComponent<PhotonTransformViewClassic>();
        InitStartOfAllStates();
        SyncTransform();
    }

    void Update()
    {
        AILogicUpdate();
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

    void InitStartOfAllStates()
    {
        foreach (var AvailableState in availableStates)
        {
            AvailableState.Value.Start();
        }
    }

    void AILogicUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (currentState == null)
            {
                currentState = availableStates.Values.First();
            }
            //return own type or another type
            var nextState = currentState?.Tick();

            //new state != currentState
            if (nextState != null && nextState != currentState?.GetType())
            {
                SwitchToNextState(nextState);
                Debug.LogWarning("My State: " + nextState);
            }
        }
    }

    void SyncTransform()
    {
        if (!PhotonNetwork.IsMasterClient)
            PV.enabled = true;
        else
            PV.enabled = false;
    }
}
