using System.Collections.Generic;
using Hadal.AudioSystem;
using Tenshi;
using UnityEngine;

namespace Hadal.AI
{
    public class AIAudioBank : MonoBehaviour
    {
        [Header("Bank")]
        [SerializeField] private List<AudioEventType> audioAssetList = new List<AudioEventType>();
        [SerializeField] private AIBrain brain;
        [SerializeField, ReadOnly] private AudioSource source2D;

        private void Awake() => source2D = gameObject.GetOrAddComponent<AudioSource>();

        /// <summary> Plays the sound delegated by the enum as 3D SFX at the given world space position. </summary>
        public bool Play3D(AISound soundType, Transform followTransform)
        {
            AudioEventData asset = GetAudioAssetOfType(soundType);
            if (asset == null)
                return false;

            brain.Send_PlayAudio(is3D: true, soundType);
            return asset.Play(followTransform);
        }

        /// <summary> Plays the sound delegated by the enum as 2D SFX. </summary>
        public void Play2D(AISound soundType)
        {
            AudioEventData asset = GetAudioAssetOfType(soundType);
            if (asset == null)
                return;
            
            brain.Send_PlayAudio(is3D: false, soundType);
            asset.Play(source2D);
        }

        private AudioEventData GetAudioAssetOfType(AISound soundType)
        {
            AudioEventData asset = null;
            int i = -1;
            while (++i < audioAssetList.Count)
            {
                if (audioAssetList[i].associatedEnum == soundType)
                {
                    asset = audioAssetList[i].audioAsset;
                    break;
                }
            }
            return asset;
        }

        [System.Serializable]
        private class AudioEventType
        {
            public AISound associatedEnum;
            public AudioEventData audioAsset;
        }
    }

    public enum AISound
    {
        Roar = 0,
        Thresh,
        Swim,
        AmbushPounce
    }
}