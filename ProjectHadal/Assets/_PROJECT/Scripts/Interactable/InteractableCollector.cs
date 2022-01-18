using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Interactables
{
    public class InteractableCollector : MonoBehaviour
    {
        public Interactable[] interactables;
        private int idCounter = 2000;
        // Start is called before the first frame update
        void Start()
        {
            interactables = FindObjectsOfType<Interactable>();

            foreach(Interactable interactable in interactables)
            {
                interactable.SetID(idCounter);
                idCounter++;
            }
        }

    }
}
