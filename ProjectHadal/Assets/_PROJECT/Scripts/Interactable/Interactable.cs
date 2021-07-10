// Created by Jin
using Photon.Pun;
using UnityEngine;
using Hadal.Usables;
using Hadal.InteractableEvents;
using Hadal.Networking;
using ExitGames.Client.Photon;

namespace Hadal.Interactables
{
    public class Interactable : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool ableToInteract;
        [SerializeField] private InteractionType interactionType;
        [SerializeField] private int interactableID;
        NetworkEventManager neManager = NetworkEventManager.Instance;

        private void Start()
        {
            neManager?.AddListener(ByteEvents.PLAYER_INTERACT, REInteract);
        }

        public void Interact(int viewID)
        {
            if (!ableToInteract)
                return;

            InteractableEventManager.Instance.InvokeInteraction(interactionType);
            ableToInteract = false;

            //send event 
            neManager?.RaiseEvent(ByteEvents.PLAYER_INTERACT, interactableID);
        }

        public void REInteract(EventData obj)
        {
            Debug.LogWarning("YO I RECEIVED EVENT");
            if (obj.Code == (byte)ByteEvents.PLAYER_INTERACT)
            {  
                int data = (int)obj.CustomData;

                if (data == interactableID)
                {
                    ableToInteract = false;
                }
            }
        }
    }
}