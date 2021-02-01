using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public abstract class AIBaseState
{
    protected GameObject gameObject;
    protected Transform transform;
    protected NavMeshAgent agent;

    public abstract Type Tick();

    public AIBaseState(GameObject GameObject)
    {
        this.gameObject = GameObject;
        this.transform = GameObject.transform;
        this.agent = GameObject.GetComponent<NavMeshAgent>();
    }

    
}
