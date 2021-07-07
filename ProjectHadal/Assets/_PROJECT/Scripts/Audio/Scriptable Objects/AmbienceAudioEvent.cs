using UnityEngine;

namespace Hadal.AudioSystem
{
    /// <summary> Audio event meant for Ambience sounds. </summary>
    [CreateAssetMenu(menuName = "Audio Event/Ambience")]
    public class AmbienceAudioEvent : AudioEventData
    {
        [SerializeField] private AudioClip Clip;
        [SerializeField] private AudioSourceSettings Settings;
        public override string Description => "Audio event meant to play Ambience. A single Clip may be used per each audio event (refer to examples or ask the Tech for help). "
                                            + "\n\nSupports 2D Playing, [Un]Pausing, and Stopping functions.";
        private AudioSourceHandler activeHandler;
        private bool usingSimulatedHandler = false;

        /// <summary> Does nothing because ambience does not need position(?) </summary>
        public override bool Play(Vector3 position)
        {
            return false;
        }

        /// <summary> 2D Ambience playing, passed in source can be null. </summary>
        public override void Play(AudioSource source)
        {
            if (Clip == null) return;

            HandleAnyExistingHandler();

            var manager = AudioManager.Instance;
            if (manager != null)
            {
                activeHandler = manager.GetAvailableAudioSourceHandler();
                AssignSourceSettings(activeHandler.Source);
                activeHandler.PlaySource();
                return;
            }

            //! fallback
            activeHandler = GetFallbackAudioSourceHandler();
            AssignSourceSettings(activeHandler.Source);
            activeHandler.PlaySource();
        }

        public override void Pause(bool isPaused)
        {
            if (activeHandler == null) return;
            if (isPaused)
            {
                activeHandler.Pause();
                return;
            }
            activeHandler.UnPause();
        }

        public override void Stop(bool isEditor = false)
        {
            if (activeHandler == null) return;
            activeHandler.Stop();
            if (usingSimulatedHandler)
            {
                if (!isEditor) Destroy(activeHandler.gameObject);
                else DestroyImmediate(activeHandler.gameObject);
            }
            activeHandler = null;
            usingSimulatedHandler = false;
        }

        private void HandleAnyExistingHandler()
        {
            if (activeHandler != null)
                Stop(!Application.isPlaying);
        }

        private void AssignSourceSettings(AudioSource source)
        {
            Settings.AssignSettings(ref source);
            source.spatialBlend = 0f;
            source.clip = Clip;
        }

        private AudioSourceHandler GetFallbackAudioSourceHandler()
        {
            AudioSourceHandler handler = new GameObject("Simulated AudioSource handler").AddComponent<AudioSourceHandler>();
            handler.gameObject.AddComponent<AudioSource>();
            handler.RefreshSource();
            usingSimulatedHandler = true;
            return handler;
        }

        private void OnValidate() => Settings.OnValidate();
    }
}
