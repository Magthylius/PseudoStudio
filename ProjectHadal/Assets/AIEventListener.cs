using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Hadal.AI;
using Hadal.Networking;

public class AIEventListener : MonoBehaviour
{
    [SerializeField] AIBrain brain;
    NetworkEventManager neManager;

    void Start()
    {
        neManager = NetworkEventManager.Instance;
        // Debug.Log(brain);
        // Debug.Log(neManager);
        Debug.LogWarning("AI START");
        if(!neManager.IsMasterClient)
        {
            neManager.AddListener(ByteEvents.AI_GRAB_EVENT, brain.RE_AttachCarriedPlayerToMouth);
            Debug.LogWarning("AI Listener");
        }
            
    }
}
