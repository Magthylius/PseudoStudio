using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentTest : MonoBehaviour
{
    public GameObject[] planes;
    public GameObject self;
    public Vector3 direction;
    public Vector3 destination;
    public int speed;
    public float timer;
    public NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        self = gameObject;
        agent = gameObject.GetComponent<NavMeshAgent>();
        ChoosePlane();
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            timer = 3.0f;
            ChoosePlane();
        }
    }

    void ChoosePlane()
    {
        int rando = Random.Range(0, planes.Length);
        destination = planes[rando].transform.position;
        agent.SetDestination(destination);
    }

}
