using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Player;

//! Created: Jon
namespace Hadal.AI.Caverns
{
    /// <summary>
    /// Cavern tunnel data struct
    /// </summary>\
    [System.Serializable]
    public struct CavernTunnel
    {
        public CavernHandler ConnectedCavern;
        public NavPoint EntryNavPoint;
    }
    
    
    /// <summary>
    /// Used only for data handling in tunnels
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TunnelBehaviour : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] LayerMask playerLayer;

        [Header("References")]
        [SerializeField] List<CavernTunnel> connectedTunnels;
        public List<CavernTunnel> ConnectedTunnels => connectedTunnels;

        //! Internal
        List<PlayerController> playersInTunnel = new List<PlayerController>();

        void OnValidate()
        {
            foreach (CavernTunnel cavern in connectedTunnels)
            {
                if (cavern.ConnectedCavern == null) continue;
                if (!cavern.ConnectedCavern.connectedTunnels.Contains(this))
                {
                    cavern.ConnectedCavern.connectedTunnels.Add(this);
                }
            }
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

        /// <summary>
        /// Checks if two given caverns are connected with this tunnel
        /// </summary>
        /// <param name="cavernOne">First cavern</param>
        /// <param name="cavernTwo">Second cavern</param>
        /// <returns>True or false</returns>
        public bool ContainsCaverns(CavernHandler cavernOne, CavernHandler cavernTwo)
        {
            bool a = false; 
            bool b = false;
            
            foreach (var tunnel in connectedTunnels)
            {
                if (tunnel.ConnectedCavern == cavernOne) a = true;
                if (tunnel.ConnectedCavern == cavernTwo) b = true;

                if (a && b) return true;
            }

            return false;
        }

        public CavernTunnel GetCavernTunnel(CavernHandler specifiedCavern)
        {
            foreach (var tunnel in connectedTunnels)
            {
                if (tunnel.ConnectedCavern == specifiedCavern)
                {
                    return tunnel;
                }
            }

            return new CavernTunnel();
        }
        
        public List<PlayerController> GetPlayersInTunnel => playersInTunnel;
    }
}
