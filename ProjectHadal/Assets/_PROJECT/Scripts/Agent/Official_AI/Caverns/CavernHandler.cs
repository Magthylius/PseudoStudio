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
    [RequireComponent(typeof(Collider))]
    public class CavernHandler : MonoBehaviour
    {
        CavernManager manager;

        public CavernTag cavernTag;

        [SerializeField] LayerMask playerMask;
        [SerializeField] LayerMask aiMask;
        [SerializeField] List<AmbushPointBehaviour> ambushPoints;

        [ReadOnly] public List<TunnelBehaviour> connectedTunnels;
        [ReadOnly] public List<CavernHandler> connectedCaverns;
        
        public event CavernHandlerPlayerReturn PlayerEnteredCavernEvent;
        public event CavernHandlerPlayerReturn PlayerLeftCavernEvent;
        public event CavernHandlerAIReturn AIEnteredCavernEvent;
        public event CavernHandlerAIReturn AILeftCavernEvent;

        int relativeDistanceCost = 0;

        new Collider collider;
        List<PlayerController> playersInCavern;
        
        void OnValidate()
        {
            collider = GetComponent<Collider>();
            collider.isTrigger = true;

            manager = FindObjectOfType<CavernManager>();
            manager.InjectHandler(this);
        }

        void Awake()
        {
            
            PlayerEnteredCavernEvent += manager.OnPlayerEnterCavern;
            PlayerLeftCavernEvent += manager.OnPlayerLeftCavern;
            AIEnteredCavernEvent += manager.OnAIEnterCavern;
            AILeftCavernEvent += manager.OnAILeaveCavern;

            playersInCavern = new List<PlayerController>();
        }

        void OnEnable()
        {
            collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        void OnDestroy()
        {
            PlayerEnteredCavernEvent -= manager.OnPlayerEnterCavern;
            PlayerLeftCavernEvent -= manager.OnPlayerLeftCavern;
            AIEnteredCavernEvent -= manager.OnAIEnterCavern;
            AILeftCavernEvent -= manager.OnAILeaveCavern;
        }

        void OnTriggerEnter(Collider other)
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

        void OnTriggerExit(Collider other)
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
            foreach(TunnelBehaviour tunnel in connectedTunnels)
            {
                foreach (CavernHandler cavern in tunnel.ConnectedCaverns)
                {
                    if (!connectedCaverns.Contains(cavern) && cavern != this)
                        connectedCaverns.Add(cavern);
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

        public int GetPlayerCount => playersInCavern.Count;
        public List<PlayerController> GetPlayersInCavern => playersInCavern;
        public List<CavernHandler> ConnectedCaverns => connectedCaverns;
    }
}
