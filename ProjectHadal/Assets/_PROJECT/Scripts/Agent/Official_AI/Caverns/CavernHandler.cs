using Hadal.Player;
using System.Collections;
using System.Collections.Generic;
using Tenshi.UnitySoku;
using UnityEngine;

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
        
        public event CavernHandlerReturn PlayerEnteredCavernEvent;
        public event CavernHandlerReturn PlayerLeftCavernEvent;
        public event CavernHandlerAIReturn AIEnteredCavernEvent;
        public event CavernHandlerAIReturn AILeftCavernEvent;

        new Collider collider;
        List<PlayerController> playersInCavern;
        
        void OnValidate()
        {
            collider = GetComponent<Collider>();
            collider.isTrigger = true; 
        }

        void Awake()
        {
            manager = FindObjectOfType<CavernManager>();
            manager.InjectHandler(this);
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

                PlayerEnteredCavernEvent.Invoke(data);
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

        public int GetPlayerCount => playersInCavern.Count;
        public List<PlayerController> GetPlayersInCavern => playersInCavern;
    }
}
