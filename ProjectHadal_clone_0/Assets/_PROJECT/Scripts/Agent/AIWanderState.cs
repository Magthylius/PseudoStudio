using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIWanderState : AIBaseState
{
    public AgentAI AgentAI;

    Vector3 agentDestination;

    float nextDestinationTimer;
    bool hasReachDestination;

   public override Type Start()
    {
        nextDestinationTimer = 10.0f;
        hasReachDestination = false;
        agent.SetDestination(GetRandomPoint());
        return null;
    }

    public AIWanderState(AgentAI _AgentAI) : base(_AgentAI.gameObject)
    {
        AgentAI = _AgentAI;
    }

    public override Type Tick()
    {
        Transform chaseTarget = CheckForAggro();
        if (chaseTarget != null)
        {
            AgentAI.SetTarget(chaseTarget);
            return typeof(AIChaseState);
        }

        CheckAgentReachedDestination();

        nextDestinationTimer -= Time.deltaTime;
        if (nextDestinationTimer < 0.0f)
        {
            nextDestinationTimer = 10.0f;
            SetNextAgentDestination();
        }

        return null;
    }

    void SetNextAgentDestination()
    {
        int rando;
        rando = UnityEngine.Random.Range(0, 1);
        if ( rando < 0.8)
        {
            agentDestination = AgentAI.mapPlanes[0].transform.position;
            agent.SetDestination(agentDestination);
        }
        else
        {
            agentDestination = AgentAI.mapPlanes[1].transform.position;
            agent.SetDestination(agentDestination);
        }
    }

    Vector3 result;
    Vector3 GetRandomPoint()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = UnityEngine.Random.insideUnitSphere * 10.0f;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomPoint, out hit, 100.0f, NavMesh.AllAreas);
            result = hit.position;
        }
        return result;
    }

    void CheckAgentReachedDestination()
    {

        if (agent.remainingDistance <= 0.1f)
        {
            hasReachDestination = true;
            agent.SetDestination(GetRandomPoint());
        }
        else
            hasReachDestination = false;
    }


    Quaternion startingAngle = Quaternion.AngleAxis(-60, Vector3.up);
    Quaternion stepAngle = Quaternion.AngleAxis(5, Vector3.up);

    private Transform CheckForAggro()
    {
        float aggroRadius = 5f;

        RaycastHit hit;
        var angle = transform.rotation * startingAngle;
        var direction = angle * Vector3.forward;
        var pos = transform.position;
        for (var i = 0; i < 24; i++)
        {
            if (Physics.Raycast(pos, direction, out hit, aggroRadius))
            {
                var player = hit.collider.GetComponent<Rigidbody>();
                if (player != null)
                {
                    Debug.DrawRay(pos, direction * hit.distance, Color.red);
                    AgentAI.SetTarget(player.transform);
                    return player.transform;
                }
                else
                {
                    Debug.DrawRay(pos, direction * hit.distance, Color.yellow);
                }
            }
            else
            {
                Debug.DrawRay(pos, direction * aggroRadius, Color.white);
            }
            direction = stepAngle * direction;

        }

        //RaycastHit hit;
        //bool isHit = Physics.BoxCast(transform.position + (transform.forward * 1), transform.lossyScale, transform.forward * 2, out hit, transform.rotation);
        //DrawBoxCastBox(transform.position + (transform.forward * 1), transform.lossyScale, transform.rotation, transform.forward * 2, hit.distance, Color.blue);
        //if (isHit)
        //{
        //    var player = hit.collider.GetComponent<AgentNavigate>();
        //    Debug.DrawRay(transform.position, transform.forward, Color.red);
        //    AgentAI.SetTarget(player.transform);
        //}
        //else
        //{
        //    Debug.DrawRay(transform.position, transform.forward, Color.green);
        //}

        return null;
    }

    #region DrawDebugBox
    public static void DrawBoxCastBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance, Color color)
    {
        direction.Normalize();
        Box bottomBox = new Box(origin, halfExtents, orientation);
        Box topBox = new Box(origin + (direction * distance), halfExtents, orientation);

        Debug.DrawLine(bottomBox.backBottomLeft, topBox.backBottomLeft, color);
        Debug.DrawLine(bottomBox.backBottomRight, topBox.backBottomRight, color);
        Debug.DrawLine(bottomBox.backTopLeft, topBox.backTopLeft, color);
        Debug.DrawLine(bottomBox.backTopRight, topBox.backTopRight, color);
        Debug.DrawLine(bottomBox.frontTopLeft, topBox.frontTopLeft, color);
        Debug.DrawLine(bottomBox.frontTopRight, topBox.frontTopRight, color);
        Debug.DrawLine(bottomBox.frontBottomLeft, topBox.frontBottomLeft, color);
        Debug.DrawLine(bottomBox.frontBottomRight, topBox.frontBottomRight, color);

        DrawBox(bottomBox, color);
        DrawBox(topBox, color);
    }

    public struct Box
    {
        public Vector3 localFrontTopLeft { get; private set; }
        public Vector3 localFrontTopRight { get; private set; }
        public Vector3 localFrontBottomLeft { get; private set; }
        public Vector3 localFrontBottomRight { get; private set; }
        public Vector3 localBackTopLeft { get { return -localFrontBottomRight; } }
        public Vector3 localBackTopRight { get { return -localFrontBottomLeft; } }
        public Vector3 localBackBottomLeft { get { return -localFrontTopRight; } }
        public Vector3 localBackBottomRight { get { return -localFrontTopLeft; } }

        public Vector3 frontTopLeft { get { return localFrontTopLeft + origin; } }
        public Vector3 frontTopRight { get { return localFrontTopRight + origin; } }
        public Vector3 frontBottomLeft { get { return localFrontBottomLeft + origin; } }
        public Vector3 frontBottomRight { get { return localFrontBottomRight + origin; } }
        public Vector3 backTopLeft { get { return localBackTopLeft + origin; } }
        public Vector3 backTopRight { get { return localBackTopRight + origin; } }
        public Vector3 backBottomLeft { get { return localBackBottomLeft + origin; } }
        public Vector3 backBottomRight { get { return localBackBottomRight + origin; } }

        public Vector3 origin { get; private set; }

        public Box(Vector3 origin, Vector3 halfExtents, Quaternion orientation) : this(origin, halfExtents)
        {
            Rotate(orientation);
        }
        public Box(Vector3 origin, Vector3 halfExtents)
        {
            this.localFrontTopLeft = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
            this.localFrontTopRight = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
            this.localFrontBottomLeft = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
            this.localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);

            this.origin = origin;
        }


        public void Rotate(Quaternion orientation)
        {
            localFrontTopLeft = RotatePointAroundPivot(localFrontTopLeft, Vector3.zero, orientation);
            localFrontTopRight = RotatePointAroundPivot(localFrontTopRight, Vector3.zero, orientation);
            localFrontBottomLeft = RotatePointAroundPivot(localFrontBottomLeft, Vector3.zero, orientation);
            localFrontBottomRight = RotatePointAroundPivot(localFrontBottomRight, Vector3.zero, orientation);
        }
    }

    static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 direction = point - pivot;
        return pivot + rotation * direction;
    }

    public static void DrawBox(Box box, Color color)
    {
        Debug.DrawLine(box.frontTopLeft, box.frontTopRight, color);
        Debug.DrawLine(box.frontTopRight, box.frontBottomRight, color);
        Debug.DrawLine(box.frontBottomRight, box.frontBottomLeft, color);
        Debug.DrawLine(box.frontBottomLeft, box.frontTopLeft, color);

        Debug.DrawLine(box.backTopLeft, box.backTopRight, color);
        Debug.DrawLine(box.backTopRight, box.backBottomRight, color);
        Debug.DrawLine(box.backBottomRight, box.backBottomLeft, color);
        Debug.DrawLine(box.backBottomLeft, box.backTopLeft, color);

        Debug.DrawLine(box.frontTopLeft, box.backTopLeft, color);
        Debug.DrawLine(box.frontTopRight, box.backTopRight, color);
        Debug.DrawLine(box.frontBottomRight, box.backBottomRight, color);
        Debug.DrawLine(box.frontBottomLeft, box.backBottomLeft, color);
    }
    #endregion

}
