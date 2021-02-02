using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NicholasAI.GGJAI
{
    public class AgentIdle : MonoBehaviour
    {
        GameObject agentAnimal;
        [SerializeField] GameObject[] mapPlanes;

        [SerializeField] bool agentChangeRot;
        [SerializeField] float agentTimer;
        [SerializeField] float agentChoosePlaneTimer;
        Vector3 destination;
        NavMeshAgent agent;


        // Start is called before the first frame update
        void Start()
        {
            agentAnimal = gameObject;
            agent = gameObject.GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
            if (agentChangeRot == false)
            {
                //transform.position -= new Vector3(0.0f, 0.0f, 0.1f);
                transform.position = Vector3.Lerp(transform.position, transform.position + UnityEngine.Random.insideUnitSphere * 1.0f, 5);
            }
 
            else
            {
                //transform.position += new Vector3(0.0f, 0.0f, 0.1f);
                transform.position = Vector3.Lerp(transform.position,transform.position + UnityEngine.Random.insideUnitSphere * 1.0f, 5);
            }
                

            agentTimer -= Time.deltaTime;
            agentChoosePlaneTimer -= Time.deltaTime;

            if (agentTimer < 0.0f)
            {
                agentTimer = 0.3f;
                    agentChangeRot = !agentChangeRot;
                
            }

            if(agentChoosePlaneTimer < 0.0f)
            {
                agentChoosePlaneTimer = Random.Range(3.0f, 10.0f);
                ChoosePlane();
            }
      

        }


        void ChoosePlane()
        {
            int rando = Random.Range(0, mapPlanes.Length);
            destination = mapPlanes[rando].transform.position;
            agent.SetDestination(destination);
        }
    }

}
