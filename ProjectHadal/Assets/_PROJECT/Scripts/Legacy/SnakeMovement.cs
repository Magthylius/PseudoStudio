
//Created by Harry
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    public List<Transform> BodyParts = new List<Transform>(); // the segments

    public float mindistance = 0.25f; // minimum distance between segments

    public float speed = 1; // speed of segments
    public float rotationspeed = 50; // speed segmeents follow previous segment rotation

    private float dis; 
    private Transform curBodyPart;
    private Transform PrevBodyPart;

    public List<Transform> Positions; // positions move
    int NextPosIndex; 
    Transform NextPos; 

    private void Start()
    {
        NextPos = Positions[0];
    }

    private void Update()
    {
        BodyMove();
    }

    public void BodyMove() // Move 1st segment body
    {
        float curspeed = speed;
        Vector3 direction = NextPos.position - BodyParts[0].position;
        Quaternion rotation = Quaternion.identity;
        if (direction.sqrMagnitude != 0f) rotation = Quaternion.LookRotation(direction); 

        if (BodyParts[0].position == NextPos.position)
        {
            NextPosIndex++;
            if (NextPosIndex >= Positions.Count)
                NextPosIndex = 0;
            NextPos = Positions[NextPosIndex];

            direction = NextPos.position - BodyParts[0].position;
            rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            BodyParts[0].position = Vector3.MoveTowards(BodyParts[0].transform.position, NextPos.position, speed * Time.deltaTime);
            BodyParts[0].rotation = Quaternion.Lerp(BodyParts[0].rotation, rotation, rotationspeed * Time.deltaTime);
            
        }
     
        for (int i = 1; i < BodyParts.Count; i++) // Move tht rest of the segment
        {
            curBodyPart = BodyParts[i];
            PrevBodyPart = BodyParts[i - 1];

            dis = Vector3.Distance(PrevBodyPart.position, curBodyPart.position);

            Vector3 newpos = PrevBodyPart.position;

            newpos.y = BodyParts[0].position.y;

            float T = Time.deltaTime * dis / mindistance * curspeed;

            if (T > 0.5f)
                T = 0.5f;
            curBodyPart.position = Vector3.Slerp(curBodyPart.position, newpos, T);
            curBodyPart.rotation = Quaternion.Slerp(curBodyPart.rotation, PrevBodyPart.rotation, T);
        }
    }

}
