using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Player;

namespace Hadal.AI.Caverns
{
    /// <summary>
    /// Used only for data handling in tunnels
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TunnelBehaviour : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] LayerMask playerLayer;

        [Header("References")]
        [SerializeField] List<CavernHandler> connectedCaverns;
        public List<CavernHandler> ConnectedCaverns => connectedCaverns;

        //! Internal
        List<PlayerController> playersInTunnel = new List<PlayerController>();

        void OnValidate()
        {
            foreach (CavernHandler cavern in connectedCaverns)
            {
                if (!cavern.connectedTunnels.Contains(this))
                {
                    cavern.connectedTunnels.Add(this);
                }
            }
        }

        void Start()
        {
        
        }

        void Update()
        {
        
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == playerLayer.value)
            {
                PlayerController player = other.GetComponent<PlayerController>();
                playersInTunnel.Add(player);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == playerLayer.value)
            {
                PlayerController player = other.GetComponent<PlayerController>();
                playersInTunnel.Remove(player);
            }
        }

        public List<PlayerController> GetPlayersInTunnel => playersInTunnel;
    }
}
