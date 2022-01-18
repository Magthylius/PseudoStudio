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
            var colliders = GetComponents<Collider>();
            int i = -1;
            while (++i < colliders.Length)
                colliders[i].isTrigger = true;
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
            var colliders = GetComponents<Collider>();
            int i = -1;
            while (++i < colliders.Length)
                colliders[i].isTrigger = false;
            
            yield return null;
            
            i = -1;
            while (++i < colliders.Length)
                colliders[i].isTrigger = true;
            
            parentCavern.SetCavernInitialize(true);
        }
    }
}
