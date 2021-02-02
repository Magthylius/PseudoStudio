// Created by Jin
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractCube : Interactable
{
    public Material deflt;
    public Material cubeColor;
    private bool isSwap;

    public override void Interact()
    {
        gameObject.GetComponent<Renderer>().material = isSwap ? deflt : cubeColor;
        isSwap = !isSwap;
    }
}
