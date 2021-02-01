using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class AgentAI : MonoBehaviour
{
    public Transform Target { get; private set; }
    public NavMeshAgent agent { get; private set; }

    public GameObject[] mapPlanes;

    public AIStateMachine StateMachine => GetComponent<AIStateMachine>();

    private void Awake()
    {
        InitStateMachine();
    }

    void Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
       
    }

    private void Update()
    {
        
    }

    private void InitStateMachine()
    {
        var states = new Dictionary<Type, AIBaseState>()
        {
            {typeof(AIWanderState), new AIWanderState(this) },
            {typeof(AIChaseState), new AIChaseState(this) }
        };
        StateMachine.SetState(states);

    }

    public void SetTarget(Transform target)
    {
        Target = target;
    }

}