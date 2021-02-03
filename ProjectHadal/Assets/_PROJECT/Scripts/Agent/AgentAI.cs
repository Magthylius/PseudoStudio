using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using System.IO;
using Photon.Pun;

public class AgentAI : MonoBehaviour
{
    public Transform Target { get; private set; }
    public NavMeshAgent Agent { get; private set; }

    public GameObject[] mapPlanes;

    public AIStateMachine StateMachine { get; private set; }

    private void Awake()
    {
        StateMachine = GetComponent<AIStateMachine>();
        InitStateMachine();
    }

    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        StorePlanes();
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


    void StorePlanes()
    {
        /*NavMeshSurface[] planesArray = GameObject.FindObjectsOfType<NavMeshSurface>();
        for (int i = 0; i < planesArray.Length; i++)
        {
            if (planesArray[i].gameObject.layer == LayerMask.NameToLayer("AINavigationLayer"))
            {
                mapPlanes[i] = planesArray[i].gameObject;
            }
        }*/
    }


}