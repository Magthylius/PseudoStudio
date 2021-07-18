// Created by Jin
using Photon.Pun;
using UnityEngine;
using Hadal.Usables;
using Hadal.InteractableEvents;
using Hadal.Networking;
using ExitGames.Client.Photon;
using System.Collections;
using Tenshi.UnitySoku;
using System.Linq;
using Tenshi;

namespace Hadal.Interactables
{
    public class Interactable : MonoBehaviour, IInteractable
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebug = true;

        [Header("General")]
        [SerializeField] private bool ableToInteract;
        [SerializeField] private bool isInteracting;
        [SerializeField] private InteractionType interactionType;
        [SerializeField] private int interactableID;
        [SerializeField] private float interactionMaintainDistance = 12f;
        [SerializeField] private float interactionWindupTime = 3f;
        private float _windupTimer = 0f;
        private Coroutine _activeTimerRoutine = null;
        

        [SerializeField] private float regenerateTimer;
        [SerializeField] private float regenerateTimerMax;
        NetworkEventManager neManager = null;
        InteractableEventManager interManager = null;

        // emissive change properties
        [SerializeField] private GameObject flareIndicator;
        [SerializeField] private MeshRenderer submarineRenderer;
        private MaterialPropertyBlock materialProp;
        private Color defaultColor;

        private void Start()
        {
            neManager = NetworkEventManager.Instance;
            interManager = InteractableEventManager.Instance;

            if(neManager)
            {
                neManager.AddListener(ByteEvents.PLAYER_INTERACTED, REInteract);
                neManager.AddListener(ByteEvents.PLAYER_INTERACTING, REInteracting);
            }    

            if (interManager)
                interManager.OnInteractConfirmation += Receive_ConfirmedInteract;

            _activeTimerRoutine = null;

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
            if (interManager)
                interManager.OnInteractConfirmation -= Receive_ConfirmedInteract;
        }

        public void Interact(int viewID)
        {
            if (!ableToInteract)
                return;

            if (isInteracting)
            {
                return;
            }

            GameObject actorPlayer = null;     

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

            if (_activeTimerRoutine != null)
            {
                if (enableDebug)
                    "Already interacting with this interactable, please wait warmly.".Msg();
                return;
            }
            
            _activeTimerRoutine = StartCoroutine(StartInteractionWindupTimer(actorPlayer));
        }

        private IEnumerator StartInteractionWindupTimer(GameObject pObject)
        {
            if (pObject == null)
            {
                "Interacting player is not found, unable to start interaction wind-up timer.".Warn();
                yield break;
            }

            //unable to be interacted when someone else is interacting. Send event to notify others.
            isInteracting = true;
            object[] content = new object[] { interactableID, isInteracting };
            neManager.RaiseEvent(ByteEvents.PLAYER_INTERACTING, content);
            //

            //! Save transform info of the player that has interacted with this script
            Transform otherTrans = pObject.transform;

            //! Setup timer values
            float sqrMaintainDist = interactionMaintainDistance.Sqr();
            ResetWindupTimer();

            while (InteractorIsWithinInteractionDistance())
            {
                bool timerFinished = ElapseWindupTimer(DeltaTime) <= 0f;
                if (timerFinished)
                {
                    HandleExitSequence(true);
                    yield break;
                }
                yield return null;
            }

            //! Interaction fail case (went out of distance)
            HandleExitSequence(false);
            yield break;

            //! Local function definitions
            float SqrDistanceBetweenInteractorAndInteractee() => (transform.position - otherTrans.position).sqrMagnitude;
            bool InteractorIsWithinInteractionDistance() => SqrDistanceBetweenInteractorAndInteractee() < sqrMaintainDist;
            void ResetWindupTimer() => _windupTimer = interactionWindupTime;
            float ElapseWindupTimer(in float deltaTime) => _windupTimer -= deltaTime;
            void HandleExitSequence(bool isSuccess)
            {
                //its no more being interacted. Send event to notify others.
                isInteracting = false;
                content = new object[] { interactableID, isInteracting };
                neManager.RaiseEvent(ByteEvents.PLAYER_INTERACTING, content);
                //

                if (isSuccess)              
                    Send_InteractionDetected();
           
                _activeTimerRoutine = null;
            }
        }

        /// <summary> This is where we send the event to interact </summary>
        private void Send_InteractionDetected()
        {
            if (interManager != null)
                interManager.InvokeInteraction(interactionType, interactableID);
        }

        private void Receive_ConfirmedInteract(int interactID)
        {
            if (interactID != interactableID)
                return;

            DisableFlare();

            if(neManager)
                neManager.RaiseEvent(ByteEvents.PLAYER_INTERACTED, interactableID);
        }

        public void REInteract(EventData obj)
        {
            if (obj.Code == (byte)ByteEvents.PLAYER_INTERACTED)
            {  
                int data = (int)obj.CustomData;

                if (data == interactableID)
                {
                    regenerateTimer = 0;
                    
                    DisableFlare();
                }
            }
        }

        public void REInteracting(EventData obj)
        {
            object[] data = (object[])obj.CustomData;
            if (obj.Code == (byte)ByteEvents.PLAYER_INTERACTING)
            {
                if ((int)data[0] == interactableID)
                {
                    isInteracting = (bool)data[1];
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
        public void SetID(int newID)
        {
            interactableID = newID;
        }

        private float DeltaTime => Time.deltaTime;
    }
}