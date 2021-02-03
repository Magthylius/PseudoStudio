using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveScript : MonoBehaviour
{
    public Material dissolveMat;
    public float amplitude = 3f;
    public float frequency = 1.2f;
    private float value = 2;

    private void Start()
    {
        dissolveMat.SetFloat("_CuttoffHeight", value);
    }

    private void FixedUpdate()
    {
        value = Mathf.Sin(Time.time * frequency) * amplitude;
        dissolveMat.SetFloat("_CuttoffHeight", value);
    }
}
