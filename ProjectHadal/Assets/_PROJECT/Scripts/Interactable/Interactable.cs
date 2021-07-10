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
        [SerializeField] private GameObject flareIndicator;

        [SerializeField] private float regenerateTimer;
        [SerializeField] private float regenerateTimerMax;
        NetworkEventManager neManager = NetworkEventManager.Instance;

        private void Start()
        {
            neManager?.AddListener(ByteEvents.PLAYER_INTERACT, REInteract);
        }

        private void Update()
        {
            if(!ableToInteract)
            {
                regenerateTimer += Time.deltaTime;

                if(regenerateTimer > regenerateTimerMax)
                {
                    regenerateTimer = 0;
                    ableToInteract = true;
                    flareIndicator.SetActive(true);
                }
            }
        }

        public void Interact(int viewID)
        {
            if (!ableToInteract)
                return;

            InteractableEventManager.Instance.InvokeInteraction(interactionType);
            flareIndicator.SetActive(false);
            ableToInteract = false;

            //send event 
            neManager?.RaiseEvent(ByteEvents.PLAYER_INTERACT, interactableID);
        }

        public void REInteract(EventData obj)
        {
            if (obj.Code == (byte)ByteEvents.PLAYER_INTERACT)
            {  
                int data = (int)obj.CustomData;

                if (data == interactableID)
                {
                    regenerateTimer = 0;
                    ableToInteract = false;
                    flareIndicator.SetActive(false);
                }
            }
        }
    }
}