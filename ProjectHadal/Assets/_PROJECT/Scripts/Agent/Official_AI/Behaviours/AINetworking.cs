using UnityEngine;
using ExitGames.Client.Photon;
using Hadal.Networking;
using Hadal.Player;
using Photon.Realtime;

namespace Hadal.AI
{
    public class AINetworking : MonoBehaviour
    {
        [SerializeField] AIBrain brain;
        NetworkEventManager neManager;
    
    
        void Start()
        {
            neManager = NetworkEventManager.Instance;
            
            if (neManager.IsMasterClient)
            {
                //! If host
                neManager.PlayerEnteredEvent += OnPlayerEnter;
                neManager.PlayerLeftEvent += OnPlayerLeft;
                neManager.RaiseEvent(ByteEvents.AI_BRAIN_DISABLE, null, SendOptions.SendReliable);
            }
            else
            {
                //! If not host
                neManager.AddListener(ByteEvents.AI_GRAB_PLAYER, RE_AttachCarriedPlayerToMouth);
                neManager.AddListener(ByteEvents.AI_RELEASE_PLAYER, RE_DetachAnyCarriedPlayer);
                neManager.AddListener(ByteEvents.AI_BRAIN_DISABLE, RE_DisableBrain);
            }
                
        }
    
        void OnPlayerEnter(Photon.Realtime.Player player)
        {
            brain.RefreshPlayerReferences();
        }
    
        void OnPlayerLeft(Photon.Realtime.Player player)
        {
            if (brain.CarriedPlayer == NetworkData.GetPlayerController(player))
            {
                brain.CarriedPlayer = null;
            }
            
            brain.RefreshPlayerReferences();
        }
        
        void RE_AttachCarriedPlayerToMouth(EventData eventData)
        {
            int data = (int)eventData.CustomData;
            
            PlayerController targetPlayer = NetworkData.GetPlayerController(data);
            if (targetPlayer != null)
            {
                brain.CarriedPlayer = targetPlayer;
                brain.CarriedPlayer.SetIsCarried(true);
                brain.AttachCarriedPlayerToMouth(true);
            }
        }
    
        void RE_DetachAnyCarriedPlayer(EventData eventData)
        {
            brain.DetachAnyCarriedPlayer();
        }

        void RE_DisableBrain(EventData eventData)
        {
            brain.DisableBrain();
        }
    }
}
