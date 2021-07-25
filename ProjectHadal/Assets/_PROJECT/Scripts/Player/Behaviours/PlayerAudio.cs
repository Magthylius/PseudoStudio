using System.Collections.Generic;
using System.Linq;
using Hadal.AudioSystem;
using Hadal.Utility;
using Hadal.Networking;
using UnityEngine;
using Tenshi.UnitySoku;
using ExitGames.Client.Photon;

namespace Hadal.Player
{
    public class PlayerAudio : MonoBehaviour, IPlayerComponent
    {
        [SerializeField] private List<AudioAsset> assets;
        [SerializeField] private List<AudioRegister> registers;
		private PlayerController _controller;

        public void Inject(PlayerController controller)
        {
            //! Initialise timers
            foreach (var reg in registers)
            {
                reg.timer = this.Create_A_Timer()
                            .WithDuration(reg.GetNewDuration)
                            .WithOnCompleteEvent(() =>
                            {
                                GetAssetOfType(reg.associatedEnum).asset.PlayOneShot(_controller.GetTarget);
                                reg.timer.RestartWithDuration(reg.GetNewDuration);
                            })
                            .WithShouldPersist(true);
                reg.timer.Pause();
            }
			
			_controller = controller;
			PlayerController.OnInitialiseComplete += InitialiseAudio;
        }

        public void DoUpdate(in float deltaTime)
        {

        }
		
		private void OnDestroy()
		{
			
		}
		
		private void InitialiseAudio(PlayerController player)
		{
			PlayerController.OnInitialiseComplete -= InitialiseAudio;
			if (player != _controller)
				return;
			
			NetworkEventManager.Instance.AddListener(ByteEvents.PLAYER_PLAY_AUDIO, Receive_PlayOneShot);
		}
		
		public void Send_PlayOneShot(AudioEventData audioAsset)
		{
			int index = GetAssetIndex(audioAsset);
			if (index == -1)
				"Audio event data scriptable object is not found in the asset list.".Warn();
			
			object[] content = new object[] { index };
			NetworkEventManager.Instance.RaiseEvent(ByteEvents.PLAYER_PLAY_AUDIO, content, SendOptions.SendReliable);
		}
		
		private void Receive_PlayOneShot(EventData eventData)
		{
			object[] content = (object[])eventData.CustomData;
			int index = (int)content[0];
			
			AudioAsset asset = assets[index];
			if (asset != null)
				asset.asset.PlayOneShot(_controller.GetTarget);
		}

        public bool EnableInRegister(PlayerSound soundType)
        {
            AudioRegister reg = registers.Where(r => r.associatedEnum == soundType).FirstOrDefault();
            if (reg != null)
            {
                reg.timer.Restart();
                return true;
            }
            return false;
        }
		
		private int GetAssetIndex(AudioEventData asset)
		{
			int i = -1;
			while (++i < assets.Count)
			{
				if (assets[i].asset == asset)
					return i;
			}
			return -1;
		}

        private AudioAsset GetAssetOfType(PlayerSound soundType)
        {
            foreach (var asset in assets)
            {
                if (asset.associatedEnum == soundType)
                    return asset;
            }
            return null;
        }

        [System.Serializable]
        private class AudioAsset
        {
            public PlayerSound associatedEnum;
            public AudioEventData asset;
        }

        [System.Serializable]
        private class AudioRegister
        {
            [HideInInspector] public Timer timer;
            public PlayerSound associatedEnum;
            [MinMaxSlider(0f, 120f)] public Vector2 durationRange;

            public float GetNewDuration => Random.Range(durationRange.x, durationRange.y);
            public void RestartTimer() => timer.Restart();
        }
    }

    public enum PlayerSound
    {
        Informer_Whalesong = 0,
		Networked_Others
    }
}
