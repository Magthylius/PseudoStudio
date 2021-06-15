using System.Collections;
using System.Collections.Generic;
using Hadal.AI.Caverns;
using UnityEngine;
using ReadOnly = NaughtyAttributes.ReadOnlyAttribute;

namespace Hadal.AI.Information
{
    public class AIPackageInfo : MonoBehaviour
    {
        private PointNavigationHandler navHandler;
        private AIBrain brain;
        
        [Header("Settings")] 
        [Tooltip("Disable update to save editor frames")] 
        [SerializeField] private bool enableUpdate = true;
        [SerializeField] private float updateDelay = 1f;

        [Header("AI Brain")] 
        [SerializeField, ReadOnly] private CavernTag targetCavern;
        [SerializeField, ReadOnly] private CavernTag currentCavern;
        [SerializeField, ReadOnly] private BrainState state;
        
        [Header("Point Nav Handler")]
        [SerializeField, ReadOnly] private NavPoint currentPoint;
        [SerializeField, ReadOnly] private float obstacleCheckTimer;
        [SerializeField, ReadOnly] private float timeoutTimer;
        [SerializeField, ReadOnly] private float lingerTimer;
        [SerializeField, ReadOnly] private float speedMultiplier;
        [SerializeField, ReadOnly] private List<NavPoint> navPoints;
        [SerializeField, ReadOnly] private List<Vector3> repulsionPoints;
        [SerializeField, ReadOnly] private bool hasReachedPoint;
        [SerializeField, ReadOnly] private bool canTimeout;
        [SerializeField, ReadOnly] private bool canAutoSelectNavPoints;
        [SerializeField, ReadOnly] private bool isOnCustomPath;
        [SerializeField, ReadOnly] private bool isChasingAPlayer;
        [SerializeField, ReadOnly] private bool canPath;
        
        
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
            } while (navHandler == null || brain == null || brain.TargetMoveCavern == null);
            
            StartUpdate();
        }
        
        IEnumerator UpdateAllData()
        {
            while (enableUpdate)
            {
                //! AIBrain
                targetCavern = brain.TargetMoveCavern.cavernTag;
                
                if (brain.CavernManager.GetHandlerOfAILocation)
                    currentCavern = brain.CavernManager.GetHandlerOfAILocation.cavernTag;
                else 
                    currentCavern = CavernTag.Invalid;
                
                state = brain.GetState;

                //! NavHandler
                obstacleCheckTimer = navHandler.Data_ObjstacleCheckTimer;
                timeoutTimer = navHandler.Data_TimeoutTimer;
                lingerTimer = navHandler.Data_LingerTimer;
                speedMultiplier = navHandler.Data_SpeedMultiplier;
                navPoints = navHandler.Data_NavPoints;
                repulsionPoints = navHandler.Data_RepulsionPoints;
                hasReachedPoint = navHandler.Data_HasReachedPoint;
                canTimeout = navHandler.Data_CanTimeOut;
                canAutoSelectNavPoints = navHandler.Data_CanAutoSelectNavPoints;
                isOnCustomPath = navHandler.Data_IsOnCustomPath;
                isChasingAPlayer = navHandler.Data_IsChasingAPlayer;
                canPath = navHandler.Data_CanPath;
                currentPoint = navHandler.Data_CurrentPoint;

                yield return new WaitForSeconds(updateDelay);
            }

            yield return null;
        }
    }
}
