using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentNavigate : MonoBehaviour
{

    void Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var agent = GetComponent<NavMeshAgent>();
                agent.SetDestination(hit.point);
            }
            
        }
    }

}