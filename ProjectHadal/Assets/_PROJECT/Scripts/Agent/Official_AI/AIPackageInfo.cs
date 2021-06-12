using System.Collections;
using System.Collections.Generic;
using Hadal.AI.Caverns;
using UnityEngine;

namespace Hadal.AI.Information
{
    public class AIPackageInfo : MonoBehaviour
    {
        private PointNavigationHandler navHandler;
        private AIBrain brain;

        [Header("Settings")] 
        [SerializeField] private bool enableUpdate = true;
        [SerializeField] private float updateDelay = 1f;

        [Header("Data Display")] 
        [SerializeField, NaughtyAttributes.ReadOnly] private CavernTag targetCavern;
        
        void Start()
        {
            StartCoroutine(TryInitialize());
        }

        void StartUpdate()
        {
            StartCoroutine(UpdateAllData());
        }

        IEnumerator TryInitialize()
        {
            do
            {
                brain = GetComponentInChildren<AIBrain>();
                navHandler = GetComponentInChildren<PointNavigationHandler>();
                yield return null;
            } while (navHandler == null || brain == null);
            
            StartUpdate();
        }
        
        IEnumerator UpdateAllData()
        {
            while (enableUpdate)
            {
                if (brain.TargetMoveCavern != null) targetCavern = brain.TargetMoveCavern.cavernTag;
                yield return new WaitForSeconds(updateDelay);
            }

            yield return null;
        }
    }
}
