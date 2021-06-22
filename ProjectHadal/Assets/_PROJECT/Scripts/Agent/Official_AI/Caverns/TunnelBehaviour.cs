using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Player;
using NaughtyAttributes;
using UnityEngine.PlayerLoop;

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
    
    public class TunnelBehaviour : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] LayerMask playerLayer;

        [Header("References")]
        [SerializeField] List<CavernTunnel> connectedTunnels;
        public List<CavernTunnel> ConnectedTunnels => connectedTunnels;

        //! Internal
        List<PlayerController> playersInTunnel = new List<PlayerController>();
        private bool aiInsideTunnel = false;

        void OnValidate()
        {
            UpdateConnectedTunnels();
        }

        [Button("Update Connected Tunnels")]
        void UpdateConnectedTunnels()
        {
            foreach (CavernTunnel cavern in connectedTunnels)
            {
                if (cavern.ConnectedCavern == null) continue;
                
                if (!cavern.ConnectedCavern.connectedTunnels.Contains(this))
                    cavern.ConnectedCavern.connectedTunnels.Add(this);

                cavern.EntryNavPoint.SetCavernTag(cavern.ConnectedCavern.cavernTag);
            }
        }
        

        public void TriggerEntry(AIBrain ai)
        {
            if (!aiInsideTunnel)
            {
                aiInsideTunnel = true;
                CavernManager.Instance.OnAIEnterTunnel(this);
            }
        }

        public void TriggerEntry(PlayerController player)
        {
            if (!playersInTunnel.Contains(player))
            {
                playersInTunnel.Add(player);
                CavernManager.Instance.OnPlayerEnterTunnel(new TunnelPlayerData(this, player));
            }
        }

        public void TriggerExit(AIBrain ai)
        {
            if (aiInsideTunnel)
            {
                aiInsideTunnel = false;
                CavernManager.Instance.OnAILeftTunnel(this);
            }
        }

        public void TriggerExit(PlayerController player)
        {
            if (playersInTunnel.Contains(player))
            {
                playersInTunnel.Remove(player);
                CavernManager.Instance.OnPlayerLeftTunnel(new TunnelPlayerData(this, player));
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

        public List<CavernHandler> GetConnectedCaverns()
        {
            List<CavernHandler> returnList = new List<CavernHandler>();
            foreach (CavernTunnel cT in connectedTunnels)
            {
                returnList.Add(cT.ConnectedCavern);
            }

            return returnList;
        }
        public int GetPlayerCount => playersInTunnel.Count;
        public List<PlayerController> GetPlayersInTunnel => playersInTunnel;
    }
}
