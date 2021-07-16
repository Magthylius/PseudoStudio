using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.InteractableEvents
{
    public enum InteractionType
    {
        Revive_Player = 0,
        Salvage_Torpedo,
    }

    public class InteractableEventManager : MonoBehaviour
    {
        public static InteractableEventManager Instance;

        public delegate void InteractEvent(InteractionType interactionType, int interactID);

        public event InteractEvent OnInteraction;

        public delegate void InteractConfirmation(int interactID);

        public event InteractConfirmation OnInteractConfirmation;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void InvokeInteraction(InteractionType interactionType, int interactID)
        {
            OnInteraction?.Invoke(interactionType, interactID);
        }

        public void InvokeInteractConfirmation(int interactID)
        {
            OnInteractConfirmation?.Invoke(interactID);
        }
    }
}

