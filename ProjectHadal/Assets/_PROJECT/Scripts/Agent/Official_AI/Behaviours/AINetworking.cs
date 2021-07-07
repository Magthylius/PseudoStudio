using UnityEngine;
using ExitGames.Client.Photon;
using Hadal.Networking;
using Hadal.Player;
using Photon.Realtime;
using Photon.Pun;
using Tenshi;
using Tenshi.UnitySoku;

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
                neManager.AddListener(ByteEvents.AI_RECEIVE_DAMAGE, RE_TakeDamage);
                neManager.AddListener(ByteEvents.AI_RECEIVE_STUN, RE_TakeStun);
				neManager.AddListener(ByteEvents.AI_UPDATE_SLOW, RE_UpdateSlow);
            }
            else
            {
                //! If not host
                neManager.AddListener(ByteEvents.AI_GRAB_PLAYER, RE_AttachCarriedPlayerToMouth);
                neManager.AddListener(ByteEvents.AI_RELEASE_PLAYER, RE_DetachAnyCarriedPlayer);
                neManager.AddListener(ByteEvents.AI_BRAIN_DISABLE, RE_DisableBrain);
                neManager.AddListener(ByteEvents.AI_DEATH, RE_Death);
                neManager.AddListener(ByteEvents.PLAYER_ALL_UNALIVE, RE_AllPlayerDeath);
            }
        }

        void OnDestroy()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                neManager.PlayerEnteredEvent -= OnPlayerEnter;
                neManager.PlayerLeftEvent -= OnPlayerLeft;
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
            int targetViewID = (int)eventData.CustomData;
            
            PlayerController targetPlayer = NetworkData.GetPlayerController(targetViewID);
            if (targetPlayer != null)
            {
                brain.CurrentTarget = targetPlayer;
                brain.TryCarryTargetPlayer();
                // brain.CarriedPlayer = targetPlayer;
                // brain.CarriedPlayer.SetIsCarried(true);
                // brain.AttachCarriedPlayerToMouth(true);
            }
        }
    
        void RE_DetachAnyCarriedPlayer(EventData eventData)
        {
            brain.DetachAnyCarriedPlayer();
            brain.CurrentTarget = null;
        }

        void RE_DisableBrain(EventData eventData)
        {
            brain.DisableBrain();
        }

        void RE_TakeDamage(EventData eventData)
        {
            brain.HealthManager.TakeDamage((int)eventData.CustomData);
        }

        void RE_TakeStun(EventData eventData)
        {
            //Debug.LogWarning("I kena fucking stun chibai");
            brain.HealthManager.TryStun((float)eventData.CustomData);
        }

        void RE_Death(EventData eventData)
        {
            brain.HealthManager.Death();
        }

        void RE_AllPlayerDeath(EventData eventData)
        {
            brain.GameHandler.PlayersLoseGame();
        }

        void RE_UpdateSlow(EventData eventData)
        {
            int changeAmount = eventData.CustomData.AsInt();
            brain.HealthManager.UpdateSlowStacks(changeAmount, false);
			
			//$"Updated Slow by event. Current stacks are {brain.HealthManager.CurrentClampedSlowStacks} (exccess: {brain.HealthManager.ExcessSlowStacks}); Max Velocity is now {brain.NavigationHandler.MaxVelocity}.".Msg();
        }
    }
}
