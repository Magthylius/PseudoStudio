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
        

        [SerializeField] private float regenerateTimer;
        [SerializeField] private float regenerateTimerMax;
        NetworkEventManager neManager = NetworkEventManager.Instance;

        // emissive change properties
        [SerializeField] private GameObject flareIndicator;
        [SerializeField] private MeshRenderer submarineRenderer;
        private MaterialPropertyBlock materialProp;
        private Color defaultColor;

        private void Start()
        {
            if(neManager)
                neManager.AddListener(ByteEvents.PLAYER_INTERACT, REInteract);

            InteractableEventManager.Instance.OnInteractConfirmation += ConfirmedInteract;

            //! initialise property block (needs to use a renderer, since it has the functions to set up)
            materialProp = new MaterialPropertyBlock();
            if (materialProp != null) submarineRenderer.GetPropertyBlock(materialProp);
            defaultColor = materialProp.GetColor("_EmissiveColor");
        }

        private void Update()
        {
            if(!ableToInteract)
            {
                regenerateTimer += Time.deltaTime;

                if(regenerateTimer > regenerateTimerMax)
                {
                    regenerateTimer = 0;
                    EnableFlare();
                }
            }
        }

        private void OnDestroy()
        {
            InteractableEventManager.Instance.OnInteractConfirmation -= ConfirmedInteract;
        }

        public void Interact(int viewID)
        {
            if (!ableToInteract)
                return;

            GameObject actorPlayer;     

            foreach(GameObject gO in NetworkEventManager.Instance.PlayerObjects)
            {
                if(!gO)
                {
                    return;
                }

                if(gO.GetComponentInChildren<PhotonView>().ViewID == viewID)
                {
                    actorPlayer = gO;
                }
            }

            //This is where we send the event to interact
            InteractableEventManager.Instance.InvokeInteraction(interactionType, interactableID);
        }

        public void ConfirmedInteract(int interactID)
        {
            if (interactID != interactableID)
                return;

            DisableFlare();

            if(neManager)
                neManager.RaiseEvent(ByteEvents.PLAYER_INTERACT, interactableID);
        }

        public void REInteract(EventData obj)
        {
            if (obj.Code == (byte)ByteEvents.PLAYER_INTERACT)
            {  
                int data = (int)obj.CustomData;

                if (data == interactableID)
                {
                    regenerateTimer = 0;
                    
                    DisableFlare();
                }
            }
        }

        private void EnableFlare()
        {
            ableToInteract = true;
            flareIndicator.SetActive(true);
            materialProp.SetColor("_EmissionColor", defaultColor);
            //Debug.Log(materialProp.GetFloat("_EmissionIntensity"));
            submarineRenderer.SetPropertyBlock(materialProp);
        }
        private void DisableFlare()
        {
            ableToInteract = false;
            flareIndicator.SetActive(false);
            materialProp.SetColor("_EmissionColor", Color.black);
            //Debug.Log(materialProp.GetFloat("_EmissionIntensity"));
            submarineRenderer.SetPropertyBlock(materialProp);
        }
        public void setID(int newID)
        {
            interactableID = newID;
        }
    }
}