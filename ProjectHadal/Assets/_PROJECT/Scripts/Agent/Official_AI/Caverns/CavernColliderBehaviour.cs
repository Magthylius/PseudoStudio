using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.Caverns
{
    public delegate void ColliderTriggerEvent(Collider collider);
    
    [RequireComponent(typeof(Collider))]
    public class CavernColliderBehaviour : MonoBehaviour
    {
        private CavernHandler parentCavern;
        public event ColliderTriggerEvent TriggerEnteredEvent;
        public event ColliderTriggerEvent TriggerLeftEvent;

        void OnValidate()
        {
            GetComponent<Collider>().isTrigger = true;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            TriggerEnteredEvent?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerLeftEvent?.Invoke(other);
        }

        public void StartColliderRecheck(CavernHandler injectedCavern)
        {
            parentCavern = injectedCavern;
            StartCoroutine(ColliderRecheck());
        }
        
        IEnumerator ColliderRecheck()
        {
            GetComponent<Collider>().enabled = false;
            yield return null;
            GetComponent<Collider>().enabled = true;
            parentCavern.SetCavernInitialize(true);
        }
    }
}
