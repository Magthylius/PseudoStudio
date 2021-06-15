using System;
using Hadal.Player;
using System.Collections;
using System.Collections.Generic;
using Tenshi.UnitySoku;
using UnityEngine;
using NaughtyAttributes;

//! C: Jon
namespace Hadal.AI.Caverns
{
    /// <summary>
    /// Used to handle a single cavern logic. 
    /// </summary>
    public class CavernHandler : MonoBehaviour
    {
        CavernManager manager;
        
        [Header("Data")]
        [SerializeField, ReadOnly] int cavernHeuristic = -1;
        [ReadOnly] public List<TunnelBehaviour> connectedTunnels;
        [ReadOnly] public List<CavernHandler> connectedCaverns;

        [Header("Settings")] 
        public CavernColliderBehaviour cavernCollider;
        public CavernTag cavernTag;
        public bool forceFirstFrameRecheck = false;

        [SerializeField] LayerMask playerMask;
        [SerializeField] LayerMask aiMask;
        [SerializeField] List<AmbushPointBehaviour> ambushPoints;

        
        public event CavernHandlerPlayerReturn PlayerEnteredCavernEvent;
        public event CavernHandlerPlayerReturn PlayerLeftCavernEvent;
        public event CavernHandlerAIReturn AIEnteredCavernEvent;
        public event CavernHandlerAIReturn AILeftCavernEvent;
        
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
            if (forceFirstFrameRecheck) cavernCollider.StartColliderRecheck();
            
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
                
                PlayerEnteredCavernEvent?.Invoke(data);
            }
            else if (other.GetComponent<AIBrain>() != null)
            {
                AIEnteredCavernEvent?.Invoke(this);
            }
            if (other.gameObject.CompareTag("NavigationPoint"))
            {
                nPoint.CavernTag = cavernTag;
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

        
        
        public int CalculateRelativeDistanceCost(CavernHandler targetCavern)
        {
            int cost = 1;
            if (ConnectedCavernContains(targetCavern))
                return cost;
            else
            {
                int lowestCost = int.MaxValue;
                foreach(CavernHandler cavern in connectedCaverns)
                {
                    int tempCost = cavern.CalculateRelativeDistanceCost(targetCavern);
                    if (tempCost < lowestCost)
                    {
                        lowestCost = tempCost;
                    }
                }

                cost = lowestCost;
            }

            return cost;
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

        //! Data
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
