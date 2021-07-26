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
		private AmbiencePlayer _ambiencePlayer;
		
		private bool isLeviathanInJudgement;

        public void Inject(PlayerController controller)
        {
			isLeviathanInJudgement = false;
			
            //! Initialise timers
            foreach (var reg in registers)
            {
                reg.timer = this.Create_A_Timer()
                            .WithDuration(reg.GetNewDuration)
                            .WithOnCompleteEvent(() =>
                            {
								if (!isLeviathanInJudgement)
									GetAssetOfType(reg.associatedEnum).asset.PlayOneShot(_controller.GetTarget);
                                else
									"The leviathan is hangry, therefore no cute sounds :)".Msg();
								
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
			
			_ambiencePlayer = FindObjectOfType<AmbiencePlayer>();
			NetworkEventManager.Instance.AddListener(ByteEvents.PLAYER_PLAY_AUDIO, Receive_PlayOneShot);
			NetworkEventManager.Instance.AddListener(ByteEvents.AI_JUDGEMENT_EVENT, data => isLeviathanInJudgement = (bool)data.CustomData);
		}
		
		public void PlayOneShot2D(PlayerSound soundType, bool networkSound = false)
		{
			AudioAsset asset = GetAssetOfType(soundType);
			asset.asset.PlayOneShot2D();
			if (networkSound)
				Send_PlayOneShot(asset.asset, false);
		}
		
		public void PlayOneShot(PlayerSound soundType, bool networkSound = false)
		{
			AudioAsset asset = GetAssetOfType(soundType);
			asset.asset.PlayOneShot(_controller.GetTarget);
			if (networkSound)
				Send_PlayOneShot(asset.asset, true);
		}
		
		private void Send_PlayOneShot(AudioEventData audioAsset, bool is3D)
		{
			if (!_controller.IsLocalPlayer)
				return;
			
			int index = GetAssetIndex(audioAsset);
			if (index == -1)
				"Audio event data scriptable object is not found in the asset list.".Warn();
			
			object[] content = new object[] { _controller.ViewID, index, is3D };
			NetworkEventManager.Instance.RaiseEvent(ByteEvents.PLAYER_PLAY_AUDIO, content, SendOptions.SendReliable);
		}
		
		private void Receive_PlayOneShot(EventData eventData)
		{
			if (_controller.IsLocalPlayer)
				return;
			
			object[] content = (object[])eventData.CustomData;
			
			int viewID = (int)content[0];
			if (viewID != _controller.ViewID)
				return;
			
			int index = (int)content[1];
			AudioAsset asset = assets[index];
			if (asset != null)
			{
				bool is3D = (bool)content[2];
				if (is3D)
					asset.asset.PlayOneShot(_controller.GetTarget);
				else
					asset.asset.PlayOneShot2D();
			}
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
		
		public bool DisableInRegister(PlayerSound soundType)
		{
			AudioRegister reg = registers.Where(r => r.associatedEnum == soundType).FirstOrDefault();
            if (reg != null)
            {
                reg.timer.Pause();
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
		
		public AmbiencePlayer AmbiencePlayer => _ambiencePlayer;

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
		Grabbed,
		Networked_Others
    }
}
