using Hadal.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI.Caverns
{
    [RequireComponent(typeof(Collider))]
    public class CavernHandler : MonoBehaviour
    {
        new Collider collider;
        [SerializeField] LayerMask playerMask;
        [SerializeField] LayerMask aiMask;

        public CavernTag cavernTag;
        public event CavernHandlerReturn PlayerEnteredCavernEvent;
        public event CavernHandlerReturn PlayerLeftCavernEvent;

        public AIBrain aiInCavern;
        List<PlayerController> playersInCavern;
        CavernManager manager;

        void OnValidate()
        {
            collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        void OnEnable()
        {
            playersInCavern = new List<PlayerController>();
        }

        void Start()
        {
            manager = CavernManager.Instance;
            manager.InjectHandler(this);

            PlayerEnteredCavernEvent += manager.OnPlayerEnterCavern;
            PlayerLeftCavernEvent += manager.OnPlayerLeftCavern;
        }

        void OnDestroy()
        {
            PlayerEnteredCavernEvent -= manager.OnPlayerEnterCavern;
            PlayerLeftCavernEvent -= manager.OnPlayerLeftCavern;
        }

        void OnTriggerEnter(Collider other)
        {
            //! Prechecks
            NavPoint nPoint = other.GetComponent<NavPoint>();
            int layerVal = other.gameObject.layer;

            if (layerVal == playerMask.value)
            {
                PlayerController player = other.GetComponent<PlayerController>();
                playersInCavern.Add(player);
                CavernPlayerData data = new CavernPlayerData(this, player);

                PlayerEnteredCavernEvent.Invoke(data);
            }
            else if (layerVal == aiMask.value)
            {
                aiInCavern = other.GetComponent<AIBrain>();
            }
            else if (nPoint != null)
            {
                nPoint.CavernTag = cavernTag;
            }
        }

        void OnTriggerExit(Collider other)
        {
            //! Prechecks
            int layerVal = other.gameObject.layer;

            if (layerVal == playerMask.value)
            {
                PlayerController player = other.GetComponent<PlayerController>();
                playersInCavern.Remove(player);
                CavernPlayerData data = new CavernPlayerData(this, player);

                PlayerLeftCavernEvent.Invoke(data);
            }
            else if (layerVal == aiMask.value)
            {
                aiInCavern = null;
            }
        }

        public int GetPlayerCount => playersInCavern.Count;
        public List<PlayerController> GetPlayersInCavern => playersInCavern;
    }
}
