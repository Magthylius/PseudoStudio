using System.Collections.Generic;
using System.Linq;
using Hadal.AudioSystem;
using Hadal.Player;
using Tenshi;
using UnityEngine;

namespace Hadal.AI
{
    public class AIAudioBank : MonoBehaviour
    {
        [Header("Bank")]
        [SerializeField] private List<AudioEventType> audioAssetList = new List<AudioEventType>();
        [SerializeField] private AIBrain brain;
        [SerializeField] private float audioDistanceRank = 50f;
        [SerializeField, ReadOnly] private AudioSource source2D;

        private void Awake() => source2D = gameObject.GetOrAddComponent<AudioSource>();

        /// <summary> Plays the sound delegated by the enum as 3D SFX at the given world space position. </summary>
        public bool Play3D(AISound soundType, Transform followTransform)
        {
            AudioEventData asset = GetAudioAssetOfType(soundType);
            if (asset == null)
                return false;

            brain.Send_PlayAudio(AIPlayAudioType.Dimension3, soundType);
            return asset.Play(followTransform);
        }

        /// <summary> Plays the sound delegated by the enum as 2D SFX. </summary>
        public void Play2D(AISound soundType)
        {
            AudioEventData asset = GetAudioAssetOfType(soundType);
            if (asset == null)
                return;

            brain.Send_PlayAudio(AIPlayAudioType.Dimension2, soundType);
            asset.Play(source2D);
        }

        public void PlayOneShot(AISound soundType, Transform followTransform)
        {
            AudioEventData asset = GetAudioAssetOfType(soundType);
            if (asset == null)
                return;

            brain.Send_PlayAudio(AIPlayAudioType.OneShot, soundType);
            asset.PlayOneShot(followTransform);
        }

        public void PlayOneShot_RoarWithDistance(Transform followTransform)
        {
            PlayerController player = brain.Players.Where(p => p.IsLocalPlayer).FirstOrDefault();
            if (player == null)
                return;

            float sqrDist = (player.GetTarget.position - followTransform.position).sqrMagnitude;

            float rank1 = audioDistanceRank.Sqr();
            float rank2 = audioDistanceRank.Sqr() * 2f;
            float rank3 = audioDistanceRank.Sqr() * 3f;

            brain.Send_PlayAudio(AIPlayAudioType.DistanceBasedRoar, AISound.Roar_Default);
            if (sqrDist <= rank1)
                PlayOneShot(AISound.Roar_Close, followTransform);
            else if (sqrDist > rank1 && sqrDist <= rank2)
                PlayOneShot(AISound.Roar_Medium, followTransform);
            else if (sqrDist > rank3)
                PlayOneShot(AISound.Roar_Far, followTransform);
        }

        // public void StopSound(AISound soundType)
        // {
        //     AudioEventData asset = GetAudioAssetOfType(soundType);
        //     if (asset == null)
        //         return;

        //     asset.Stop();
        // }

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

    internal enum AIPlayAudioType
    {
        Dimension2 = 0,
        Dimension3,
        OneShot,
        DistanceBasedRoar
    }

    public enum AISound
    {
        Roar_Default = 0,
        Roar_Close,
        Roar_Medium,
        Roar_Far,
        CarryWarning,
        Thresh,
        Swim,
        Ambush,
        AmbushPounce,
        AmbushPlayerClose,
        Hurt,
        EggDestroyed,
        Death,
        GrabRiser,
    }
}