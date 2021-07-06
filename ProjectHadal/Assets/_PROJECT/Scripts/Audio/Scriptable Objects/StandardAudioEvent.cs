using Tenshi;
using UnityEngine;

namespace Hadal.AudioSystem
{
    /// <summary>
    /// Standard audio event meant to manage SFX playing.
    /// </summary>
    [CreateAssetMenu(menuName = "Audio Event/Standard")]
    public class StandardAudioEvent : AudioEventData
    {
        [SerializeField] private AudioClip[] Clips;
        [SerializeField] private AudioSourceSettings Settings;

        #region Locational based Play

        /// <summary> Plays audio at a world position. </summary>
        public override bool Play(Vector3 position)
        {
            if (Clips.IsNullOrEmpty()) return false;
            return RuntimePlay(position);
        }

        /// <summary> Plays an audio clip sfx through the audio manager & related runtime audio source handlers. </summary>
        private bool RuntimePlay(Vector3 position)
        {
            var manager = AudioManager.Instance;
            if (manager != null)
            {
                var handler = manager.GetAvailableAudioSourceHandler(true);
                handler.Setup(in Settings);
                handler.SetWorldPosition(position);
                handler.Source.clip = Clips.RandomElement();
                handler.PlaySource();
                return true;
            }

            return EditorPlay(GetFallbackAudioSource(), position, true);
        }

        /// <summary> Plays an audio clip that is safe to use in the editor (or when unplayed). </summary>
        /// <param name="position">Optional: Null is there is no position required.</param>
        /// <param name="destroyOnComplete">Optional: Destroy game object of the audiosource when it is done playing its clip.</param>
        private bool EditorPlay(AudioSource source, Vector3? position = null, bool destroyOnComplete = false)
        {
            if (source == null)
                source = GetFallbackAudioSource();

            int index = ArrangeSourceWithClip(ref source);
            if (position.HasValue) source.transform.position = position.Value;
            source.Play();
            
            if (destroyOnComplete) Destroy(source.gameObject, Clips[index].length);
            
            return true;
        }

        #endregion

        #region 2D based Play

        /// <summary> Plays a one shot audio clip with an audio source. </summary>
        /// <param name="source">Play with this source. If null, a new one will be created automatically.</param>
        public override void Play(AudioSource source)
        {
            if (source == null || Clips.IsNullOrEmpty()) return;

            int index = ArrangeSourceWithClip(ref source);
            source.spatialBlend = 0f; //this may be commented out if you want partial spatial sounds
            source.PlayOneShot(Clips[index]);
        }

        #endregion

        #region Utility Methods

        /// <summary> Assigns audio source with appropriate settings. Returns the index of the chosen audio clip assigned. </summary>
        private int ArrangeSourceWithClip(ref AudioSource source)
        {
            Settings.AssignSettings(ref source);
            int randomIndex = Random.Range(0, Clips.Length);
            source.clip = Clips[randomIndex];
            return randomIndex;
        }

        /// <summary> Returns an audio source on a newly instantiated game object. </summary>
        private AudioSource GetFallbackAudioSource()
        {
            AudioSource source = new GameObject("Audio Source Object").AddComponent<AudioSource>();
            return source;
        }

        #endregion

        //! No need to pause or stop for SFX sounds
        public override void Pause(bool isPaused) { }
        public override void Stop() { }
    }
}
