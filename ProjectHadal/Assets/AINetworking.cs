using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Hadal.AI;
using Hadal.Networking;
using Photon.Realtime;
using Hadal.Player;

public class AINetworking : MonoBehaviour
{
    [SerializeField] AIBrain brain;
    NetworkEventManager neManager;


    void Start()
    {
        neManager = NetworkEventManager.Instance;
        
        // Debug.Log(brain);
        // Debug.Log(neManager);
        //Debug.LogWarning("AI START");
        
        if (neManager.IsMasterClient)
        {
            //! If host
            neManager.PlayerEnteredEvent += OnPlayerEnter;
            neManager.PlayerLeftEvent += OnPlayerLeft;
            //Debug.LogWarning("Player entered event");
        }
        else
        {
            //! If not host
            neManager.AddListener(ByteEvents.AI_GRAB_PLAYER, RE_AttachCarriedPlayerToMouth);
            neManager.AddListener(ByteEvents.AI_RELEASE_PLAYER, RE_DetachAnyCarriedPlayer);
            //Debug.LogWarning("AI Listener");
        }
            
    }

    void OnPlayerEnter(Player player)
    {
        brain.RefreshPlayerReferences();
    }

    void OnPlayerLeft(Player player)
    {
        
        brain.RefreshPlayerReferences();
    }
    
    public void RE_AttachCarriedPlayerToMouth(EventData eventData)
    {
        int data = (int)eventData.CustomData;
        
        PlayerController targetPlayer = NetworkData.GetPlayerController(data);
        //Debug.LogWarning("TP: " + targetPlayer);
        
        if (targetPlayer != null)
        {
            brain.CarriedPlayer = targetPlayer;
            brain.CarriedPlayer.SetIsCarried(true);
            brain.AttachCarriedPlayerToMouth(true);
        }
    }

    public void RE_DetachAnyCarriedPlayer(EventData eventData)
    {
        brain.DetachAnyCarriedPlayer();
    }
}
