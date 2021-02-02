using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIChaseState : AIBaseState

{
    AgentAI AgentAI;
    float chaseTimer = 5.0f; 

    public AIChaseState(AgentAI _AgentAI): base(_AgentAI.gameObject)
    {
        AgentAI = _AgentAI;
    }
    public override Type Start()
    {
        chaseTimer = 5.0f;
        return null;
    }

    public override Type Tick()
    {
        if (AgentAI.Target == null)
            return typeof(AIWanderState);

        chaseTimer -= Time.deltaTime;
        if (chaseTimer < 0)
        {
            AgentAI.SetTarget(null);
            chaseTimer = 2f;
            return typeof(AIWanderState);
        }

        transform.LookAt(AgentAI.Target);
        agent.SetDestination(AgentAI.Target.position);
        
        //float distance = Vector3.Distance(transform.position, AgentAI.Target.transform.position);
        //float rangeOfAttack = 10.0f;
        //if(distance <= rangeOfAttack)
        //{
        //    //TODO: Do damage to player.
        //}
        //Debug.Log("HI");

        return null;
        
    }
}
