// Created by Jin
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class InteractCube : Interactable
{
    public Material deflt;
    public Material cubeColor;
    private bool isSwap;
    private const byte CUBE_INTERACT_EVENT = 0;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {
         if(obj.Code == CUBE_INTERACT_EVENT)
         {
            ChangeColor();
         }
    }

    public override void Interact()
    {
        ChangeColor();
        PhotonNetwork.RaiseEvent(CUBE_INTERACT_EVENT, null, RaiseEventOptions.Default, SendOptions.SendUnreliable);        
    }

    private void ChangeColor()
    {
        Material mat = isSwap ? deflt : cubeColor;
        gameObject.GetComponent<Renderer>().material = mat;
        isSwap = !isSwap; 
    }

}
