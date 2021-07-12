using System;
using Hadal.Player;
using System.Collections;
using System.Collections.Generic;
using Tenshi.UnitySoku;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

//! C: Jon
namespace Hadal.AI.Caverns
{
    /// <summary>
    /// Used to handle a single cavern logic. 
    /// </summary>
    public class CavernHandler : MonoBehaviour
    {
        CavernManager manager;
        //! used to communicate with manager if it needs a first frame recheck
        private bool cavernInitialized = false;
        
        [Header("Data")]
        [SerializeField, ReadOnly] int cavernHeuristic = -1;
        [ReadOnly] public List<TunnelBehaviour> connectedTunnels;
        [ReadOnly] public List<CavernHandler> connectedCaverns;
        [SerializeField, ReadOnly] private int playerCountInCavern;

        [Header("Settings")] 
        public CavernColliderBehaviour cavernCollider;
        public CavernTag cavernTag;
        public bool forceFirstFrameRecheck = false;
		[SerializeField] private bool autoAssignNavPoints;
        [SerializeField] LayerMask playerMask;
        [SerializeField] LayerMask aiMask;
        [SerializeField] List<AmbushPointBehaviour> ambushPoints;

        //! Events
        public event CavernHandlerPlayerReturn PlayerEnteredCavernEvent;
        public event CavernHandlerPlayerReturn PlayerLeftCavernEvent;
        public event CavernHandlerAIReturn AIEnteredCavernEvent;
        public event CavernHandlerAIReturn AILeftCavernEvent;
        
        //! Data
        List<PlayerController> playersInCavern = new List<PlayerController>();

        void OnValidate()
        {
            manager = FindObjectOfType<CavernManager>();
            manager.InjectHandler(this);
        }

        void Awake()
        {
            manager = FindObjectOfType<CavernManager>();
            manager.InjectHandler(this);
        }

        void Start()
        {
            if (forceFirstFrameRecheck) cavernCollider.StartColliderRecheck(this);
            else cavernInitialized = true;
            
            PlayerEnteredCavernEvent += manager.OnPlayerEnterCavern;
            PlayerLeftCavernEvent += manager.OnPlayerLeftCavern;
            AIEnteredCavernEvent += manager.OnAIEnterCavern;
            AILeftCavernEvent += manager.OnAILeaveCavern;

            playersInCavern = new List<PlayerController>();

            if (cavernCollider == null) GetComponentInChildren<CavernColliderBehaviour>();
            if (cavernCollider == null) Debug.LogError("Cavern collider is null!");
            else
            {
                cavernCollider.TriggerEnteredEvent += ColliderTriggerEnter;
                cavernCollider.TriggerLeftEvent += ColliderTriggerLeave;
            }
        }

        void ColliderTriggerEnter(Collider other)
        {
            //! Prechecks
            NavPoint nPoint = other.GetComponent<NavPoint>();
            int layerVal = other.gameObject.layer;

            if (other.GetComponent<PlayerController>() != null)
            {
                PlayerController player = other.GetComponent<PlayerController>();
                playersInCavern.Add(player);
                CavernPlayerData data = new CavernPlayerData(this, player);

                playerCountInCavern = GetPlayerCount;
                PlayerEnteredCavernEvent?.Invoke(data);
            }
            else if (other.GetComponent<AIBrain>() != null)
            {
                AIEnteredCavernEvent?.Invoke(this);
            }
            if (other.gameObject.CompareTag("NavigationPoint"))
            {
				if (!autoAssignNavPoints)
					return;
                
                if (nPoint.CavernTag != CavernTag.Custom_Point)
                    nPoint.SetCavernTag(cavernTag);
            }
        }

        void ColliderTriggerLeave(Collider other)
        {
            //! Prechecks
            int layerVal = other.gameObject.layer;

            if (other.GetComponent<PlayerController>() != null)
            {
                PlayerController player = other.GetComponent<PlayerController>();
                playersInCavern.Remove(player);
                CavernPlayerData data = new CavernPlayerData(this, player);
                
                playerCountInCavern = GetPlayerCount;
                PlayerLeftCavernEvent.Invoke(data);
            }
            else if (other.GetComponent<AIBrain>() != null)
            {
                if (other.GetComponent<AIBrain>() != null)
                    AILeftCavernEvent?.Invoke(this);
            }
        }

        [Button("Update connected caverns")]
        void UpdateConnectedCaverns()
        {
            connectedCaverns = new List<CavernHandler>();
            foreach(TunnelBehaviour tunnelBehav in connectedTunnels)
            {
                foreach (CavernTunnel tunnel in tunnelBehav.ConnectedTunnels)
                {
                    if (!connectedCaverns.Contains(tunnel.ConnectedCavern) && tunnel.ConnectedCavern != this)
                        connectedCaverns.Add(tunnel.ConnectedCavern);
                }
            }
        }
        
        public bool ConnectedCavernContains(CavernHandler targetCavern)
        {
            return connectedCaverns.Contains(targetCavern);
        }
        
        /// <summary>
        /// Gets both end of NavPoint of the tunnel connecting this cavern to targetCavern.
        /// </summary>
        /// <param name="targetCavern">Targeted cavern to get entry NavPoint.</param>
        /// <returns>Array of 2 NavPoints if found, null if not.</returns>
        public NavPoint[] GetEntryNavPoints(CavernHandler targetCavern)
        {
            NavPoint[] navPoints = new NavPoint[2];

            foreach (var cavernTunnel in connectedTunnels)
            {
                if (cavernTunnel.ContainsCaverns(this, targetCavern))
                {
                    navPoints[0] = cavernTunnel.GetCavernTunnel(this).EntryNavPoint;
                    navPoints[1] = cavernTunnel.GetCavernTunnel(targetCavern).EntryNavPoint;

                    if (navPoints[0] != null && navPoints[1] != null)
                        return navPoints;
                    
                    Debug.LogError("Tunnel NavPoints is null!");
                    break;
                }
                
            }

            return null;
        }

        public PlayerController GetIsolatedPlayer()
        {
            if (playersInCavern.Count == 1)
                return playersInCavern.Where(p => p != null).Single();
            
            return null;
        }

        public PlayerController GetClosestPlayerTo(Transform thisTrans)
        {
            if (playersInCavern.Count == 0)
                return null;
            
            return playersInCavern.OrderBy(p => (p.GetTarget.position - thisTrans.position).sqrMagnitude).FirstOrDefault();
        }

        //! Player enquiry
        /// <summary>
        /// Checks if there is a player in this handler with a specified view id.
        /// </summary>
        public bool HasPlayerWithViewID(in int viewID)
        {
            playersInCavern.RemoveAll(p => p == null);
            for (int i = 0; i < playersInCavern.Count; i++)
            {
                if (playersInCavern[i].ViewID == viewID)
                    return true;
            }
            return false;
        }

        //! Data
        public void SetCavernInitialize(bool newStatus) => cavernInitialized = newStatus;
        public bool IsInitialized => cavernInitialized;
        public string CavernName => gameObject.name;
        public int GetPlayerCount => playersInCavern.Count;
        public List<PlayerController> GetPlayersInCavern => playersInCavern;
        public List<CavernHandler> ConnectedCaverns => connectedCaverns;
        
        //! Heuristics
        public void SetHeuristic(int newHeuristic) => cavernHeuristic = newHeuristic;
        public void ResetHeuristic() => cavernHeuristic = -1;
        public int GetHeuristic => cavernHeuristic;
        public int GetPlayerAccountedHeuristic => cavernHeuristic + GetPlayerCount;
    }
}
