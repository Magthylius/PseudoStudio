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

        public CavernTag cavernTag;
        public event CavernHandlerReturn PlayerEnteredCavernEvent;
        public event CavernHandlerReturn PlayerLeftCavernEvent;

        //! Navpoints?

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
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                playersInCavern.Add(player);
                CavernPlayerData data = new CavernPlayerData(this, player);

                PlayerEnteredCavernEvent.Invoke(data);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                playersInCavern.Remove(player);
                CavernPlayerData data = new CavernPlayerData(this, player);

                PlayerLeftCavernEvent.Invoke(data);
            }
        }

        public int GetPlayerCount => playersInCavern.Count;
        public List<PlayerController> GetPlayersInCavern => playersInCavern;
    }
}
