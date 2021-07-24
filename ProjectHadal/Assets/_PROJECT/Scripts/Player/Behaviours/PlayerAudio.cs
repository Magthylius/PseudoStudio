using System.Collections.Generic;
using System.Linq;
using Hadal.AudioSystem;
using Hadal.Utility;
using UnityEngine;

namespace Hadal.Player
{
    public class PlayerAudio : MonoBehaviour, IPlayerComponent
    {
        [SerializeField] private List<AudioAsset> assets;
        [SerializeField] private List<AudioRegister> registers;

        public void Inject(PlayerController controller)
        {
            registers = new List<AudioRegister>();

            //! Initialise timers
            foreach (var reg in registers)
            {
                reg.timer = this.Create_A_Timer()
                            .WithDuration(reg.GetNewDuration)
                            .WithOnCompleteEvent(() =>
                            {
                                // GetAssetOfType(reg.associatedEnum).asset.Play()
                                reg.timer.RestartWithDuration(reg.GetNewDuration);
                            })
                            .WithShouldPersist(true);
                reg.timer.Pause();
            }
        }

        public void DoUpdate(in float deltaTime)
        {

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
    }
}
