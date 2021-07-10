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

        public delegate void InteractEvent(InteractionType interactionType);

        public event InteractEvent OnInteraction;

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

        public void InvokeInteraction(InteractionType interactionType)
        {
            OnInteraction?.Invoke(interactionType);
        }

    }
}

