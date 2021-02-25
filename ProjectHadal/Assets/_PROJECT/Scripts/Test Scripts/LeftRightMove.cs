using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftRightMove : MonoBehaviour
{
    [SerializeField]private float Timer;
    [SerializeField] private float ShiftTimer;
    [SerializeField] private float Speed;
    void Update()
    {
        if(Timer < ShiftTimer/2)
        {
            Timer += Time.deltaTime;
            transform.position += Vector3.left * Speed * Time.deltaTime;
        }
        else if(Timer < ShiftTimer)
        {
            Timer += Time.deltaTime;
            transform.position += Vector3.right * Speed * Time.deltaTime;
        }
        else
        {
            Timer = 0;
        }
    }
}
