using System.Linq;
using Tenshi;
using UnityEngine;

namespace Hadal.AudioSystem
{
    /// <summary> Audio event meant for SFX sounds that supports weight audio clips. </summary>
    [CreateAssetMenu(menuName = "Audio Event/Weighted SFX")]
    public class WeightedSFXAudioEvent : AudioEventData
    {
        [SerializeField] private WeightedAudioClip[] WeightedClips;
        [SerializeField] private AudioSourceSettings Settings;
        private float lastPlayTime = 0f;
        public override string Description => "Audio event meant to play SFX weight sounds. Clip variants used in this audio event may be assigned a weight value to make specific clips in the list play more or less often (refer to examples or ask the Tech for help). "
                                            + "\n\nSupports 3D Weighted Playing, and 2D Weighted Playing functions."
                                            + "\n\nNote: Preview Button will only play 2D audio for now.";

        private bool CheckForPlayTime()
        {
            if (Time.time > lastPlayTime)
            {
                lastPlayTime = Time.time + Settings.ReplayTime;
                return true;
            }
            return false;
        }

        #region Locational based Play

        public override bool Play(Transform followPosTransform)
        {
            if (WeightedClips.IsNullOrEmpty() || !CheckForPlayTime()) return false;
            return RuntimePlay(followPosTransform.position, followPosTransform);
        }

        /// <summary> Plays weighted audio at a world position. </summary>
        public override bool Play(Vector3 position)
        {
            if (WeightedClips.IsNullOrEmpty() || !CheckForPlayTime()) return false;
            return RuntimePlay(position);
        }

        /// <summary> Plays a weighted audio clip sfx through the audio manager & related runtime audio source handlers. </summary>
        private bool RuntimePlay(Vector3 position, Transform parent = null)
        {
            var manager = AudioManager.Instance;
            if (manager != null)
            {
                var handler = manager.GetAvailableAudioSourceHandler();
                handler.Setup(in Settings);
                handler.SetWorldPosition(position);
                handler.SetParent(parent);
                handler.Source.clip = GetWeightedClip();
                handler.PlaySource();
                return true;
            }

            return EditorPlay(GetFallbackAudioSource(), position, parent, true);
        }

        /// <summary> Plays a weighted audio clip that is safe to use in the editor (or when unplayed). </summary>
        /// <param name="position">Optional: Null is there is no position required.</param>
        /// <param name="destroyOnComplete">Optional: Destroy game object of the audiosource when it is done playing its clip.</param>
        private bool EditorPlay(AudioSource source, Vector3? position = null, Transform parent = null, bool destroyOnComplete = false)
        {
            if (source == null)
                source = GetFallbackAudioSource();

            var clip = ArrangeSourceWithClip(ref source);
            if (position.HasValue) source.transform.position = position.Value;
            source.transform.parent = parent;
            source.Play();
            
            if (destroyOnComplete) Destroy(source.gameObject, clip.length);
            
            return true;
        }

        #endregion

        #region 2D based Play

        /// <summary> Plays a weighted one shot audio clip with an audio source. </summary>
        /// <param name="source">Play with this source. If null, a new one will be created automatically.</param>
        public override void Play(AudioSource source)
        {
            if (WeightedClips.IsNullOrEmpty() || !CheckForPlayTime()) return;

            var clip = ArrangeSourceWithClip(ref source);
            
            var manager = AudioManager.Instance;
            if (manager != null)
            {
                var handler = manager.GetAvailableAudioSourceHandler();
                handler.Setup(in Settings);
                handler.Source.clip = clip;
                handler.Source.spatialBlend = 0f;
                handler.PlaySource();
            }
        }

        #endregion

        #region Utility Methods

        /// <summary> Assigns audio source with appropriate settings. Returns the index of the chosen audio clip assigned. </summary>
        private AudioClip ArrangeSourceWithClip(ref AudioSource source)
        {
            Settings.AssignSettings(ref source);
            var clip = GetWeightedClip();
            source.clip = clip;
            return clip;
        }

        /// <summary> Returns a weighted audio clip. </summary>
        private AudioClip GetWeightedClip()
        {
            float totalWeight = WeightedClips.Sum(wc => wc.Weight);
            float threshold = Random.Range(0f, totalWeight);
            int i = -1;
            while (++i < WeightedClips.Length)
            {
                float clipWeight = WeightedClips[i].Weight;
                if (threshold > clipWeight)
                {
                    threshold -= clipWeight;
                    continue;
                }

                return WeightedClips[i].Clip;
            }
            return null;
        }

        /// <summary> Returns an audio source on a newly instantiated game object. </summary>
        private AudioSource GetFallbackAudioSource()
        {
            AudioSource source = new GameObject("Audio Source Object").AddComponent<AudioSource>();
            return source;
        }

        #endregion

        public override void Pause(bool isPaused) { }
        public override void Stop(bool isEditor = false) { }

        private void OnValidate() => Settings.OnValidate();

        [System.Serializable]
        private struct WeightedAudioClip
        {
            public AudioClip Clip;
            public float Weight;
        }
    }
}
