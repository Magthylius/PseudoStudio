//Created by Harry
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    public List<Transform> BodyParts = new List<Transform>();

    public float mindistance = 0.25f;

    public float speed = 1;
    public float rotationspeed = 50;

    private float dis;
    private Transform curBodyPart;
    private Transform PrevBodyPart;

    private void Start()
    {

    }

    private void Update()
    {
        BodyMove();
    }

    public void BodyMove()
    {
        float curspeed = speed;

       BodyParts[0].Translate(BodyParts[0].forward * curspeed * Time.smoothDeltaTime, Space.World);

        for (int i = 1; i < BodyParts.Count; i++)
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
