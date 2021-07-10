using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hadal.AI.Caverns;
using Photon.Pun;
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
        [SerializeField, ReadOnly] private CavernTag nextCavern;
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

        [Header("Pathing")] 
        [SerializeField] private bool drawPathing = true;
        [SerializeField, ReadOnly] private List<NavPoint> pointPathList;
        [SerializeField, ReadOnly] private List<NavPoint> cachedPointPathList;
        
        
        void Start()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                enableUpdate = false;
                return;
            }
            
            StartCoroutine(TryInitialize());
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!PhotonNetwork.IsConnected || !drawPathing || !PhotonNetwork.IsMasterClient) return;
            for (int i = 0; i < cachedPointPathList.Count - 1; i++)
            {
                Gizmos.color = Color.cyan;
                if (cachedPointPathList[i] == null || cachedPointPathList[i + 1] == null) return;
                Gizmos.DrawLine(cachedPointPathList[i].GetPosition, cachedPointPathList[i + 1].GetPosition);
            }
            
            for (int i = 0; i < pointPathList.Count - 1; i++)
            {
                Gizmos.color = Color.red;
                if (pointPathList[i] == null || pointPathList[i + 1] == null) return;
                Gizmos.DrawLine(pointPathList[i].GetPosition, pointPathList[i + 1].GetPosition);
            }
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
                while (brain.CavernManager == null)
                    yield return null;

                //! AIBrain
                targetCavern = brain.TargetMoveCavern ? brain.TargetMoveCavern.cavernTag : CavernTag.Invalid;
                nextCavern = brain.NextMoveCavern ? brain.NextMoveCavern.cavernTag : CavernTag.Invalid;
                currentCavern = brain.CavernManager.GetHandlerOfAILocation ? brain.CavernManager.GetHandlerOfAILocation.cavernTag : CavernTag.Invalid;
                
                state = brain.GetState;

                //! NavHandler
                obstacleCheckTimer = navHandler.Data_ObjstacleCheckTimer;
                timeoutTimer = navHandler.Data_TimeoutTimer;
                lingerTimer = navHandler.Data_NavPointLingerTimer;
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
                pointPathList = navHandler.GetPointPath.ToList();
                cachedPointPathList = navHandler.GetCachedPointPath.ToList();

                yield return new WaitForSeconds(updateDelay);
            }

            yield return null;
        }
    }
}
