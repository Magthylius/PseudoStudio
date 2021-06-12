using System;
using System.Collections;
using System.Collections.Generic;
using Hadal.AI.Caverns;
using Hadal.Player;
using UnityEngine;

namespace Hadal.AI
{
    public enum TunnelColliderType
    {
        Entry = 0,
        Exit
    }
    
    public class TunnelColliderBehaviour : MonoBehaviour
    {
        [SerializeField] private TunnelColliderType type;
        [SerializeField] private TunnelBehaviour parentTunnel;

        private CavernManager cManager;

        private void Start()
        {
            cManager = CavernManager.Instance;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (type == TunnelColliderType.Exit) return;
            
            //print(LayerMask.LayerToName(other.gameObject.layer) + " entered");
            //print(LayerMask.LayerToName(cManager.PlayerLayer.value) + " cMan");
            //print(gameObject.name);
            if (other.gameObject.layer == cManager.PlayerLayer)
                parentTunnel.TriggerEntry(other.GetComponent<PlayerController>());
            else if (other.gameObject.layer == cManager.AILayer)
                parentTunnel.TriggerEntry(other.GetComponent<AIBrain>());
        }

        private void OnTriggerExit(Collider other)
        {
            if (type == TunnelColliderType.Entry) return;
            //print(LayerMask.LayerToName(other.gameObject.layer)   + " left");
            //print(gameObject.name);
            if (other.gameObject.layer == cManager.PlayerLayer)
                parentTunnel.TriggerExit(other.GetComponent<PlayerController>());
            else if (other.gameObject.layer == cManager.AILayer)
                parentTunnel.TriggerExit(other.GetComponent<AIBrain>());
        }

        private void OnDrawGizmosSelected()
        {
            if (type == TunnelColliderType.Entry)
                Gizmos.color = Color.yellow;
            else
                Gizmos.color = Color.red;
            
            Gizmos.DrawSphere(transform.position, 1f);
        }
    }
}
