using Tenshi;
using UnityEngine;

namespace Hadal.AudioSystem
{
    /// <summary> Audio event meant for SFX sounds. </summary>
    [CreateAssetMenu(menuName = "Audio Event/SFX")]
    public class SFXAudioEvent : AudioEventData
    {
        [SerializeField] private AudioClip[] Clips;
        [SerializeField] private AudioSourceSettings Settings;
        public override string Description => "Audio event meant to play SFX sounds. Clip variants may be used per each audio event (refer to examples or ask the Tech for help). "
                                            + "\n\nSupports 3D Playing, and 2D Playing functions. "
                                            + "\n\nNote: Preview Button will only play 2D audio for now.";

        #region Locational based Play

        public override bool Play(Transform followPosTransform)
        {
            if (Clips.IsNullOrEmpty()) return false;
            return RuntimePlay(followPosTransform.position, followPosTransform);
        }

        /// <summary> Plays audio at a world position. </summary>
        public override bool Play(Vector3 position)
        {
            if (Clips.IsNullOrEmpty()) return false;
            return RuntimePlay(position);
        }

        /// <summary> Plays an audio clip sfx through the audio manager & related runtime audio source handlers. </summary>
        private bool RuntimePlay(Vector3 position, Transform parent = null)
        {
            var manager = AudioManager.Instance;
            if (manager != null)
            {
                var handler = manager.GetAvailableAudioSourceHandler();
                handler.Setup(in Settings);
                handler.SetWorldPosition(position);
                handler.SetParent(parent);
                handler.Source.clip = Clips.RandomElement();
                handler.PlaySource();
                return true;
            }

            return EditorPlay(GetFallbackAudioSource(), position, parent, true);
        }

        /// <summary> Plays an audio clip that is safe to use in the editor (or when unplayed). </summary>
        /// <param name="position">Optional: Null is there is no position required.</param>
        /// <param name="destroyOnComplete">Optional: Destroy game object of the audiosource when it is done playing its clip.</param>
        private bool EditorPlay(AudioSource source, Vector3? position = null, Transform parent = null, bool destroyOnComplete = false)
        {
            if (source == null)
                source = GetFallbackAudioSource();

            int index = ArrangeSourceWithClip(ref source);
            if (position.HasValue) source.transform.position = position.Value;
            source.transform.parent = parent;
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
            if (Clips.IsNullOrEmpty()) return;

            AudioManager manager = AudioManager.Instance;
            if (manager != null)
            {
                AudioSourceHandler handler = manager.GetAvailableAudioSourceHandler();
                handler.Setup(in Settings);
                handler.Source.clip = Clips.RandomElement();
                handler.Source.spatialBlend = 0f;
                handler.PlaySource();
            }
            else
            {
				AudioSourceHandler handler = GetFallbackAudioSourceHandler();
                ArrangeSourceWithClip(ref handler.Source);
                handler.Source.spatialBlend = 0f;
                handler.PlaySource();
                if (Application.isPlaying)
					Destroy(handler.gameObject, handler.Source.clip.length);
				else
					handler.DestroyImmediateOnFinish = true;
            }
        }

        #endregion
		
		#region Play One Shot
		
		public override void PlayOneShot(Transform followPosTransform)
        {
			if (Clips.IsNullOrEmpty()) return;
			
            var manager = AudioManager.Instance;
            if (manager != null)
            {
                //var handler = manager.GetAvailableAudioSourceHandler();
                AudioClip randomClip = Clips.RandomElement();
                var handler = manager.GetOneShotAudioSourceHandler(randomClip);
                handler.Setup(in Settings);
				handler.SetWorldPosition(followPosTransform.position);
				handler.SetParent(followPosTransform);
				handler.Source.clip = randomClip;
                handler.Source.PlayOneShot(handler.Source.clip, Settings.Volume.RandomBetweenXY());
            }
			else
			{
				AudioSourceHandler handler = GetFallbackAudioSourceHandler();
			 	int index = ArrangeSourceWithClip(ref handler.Source);
				handler.SetWorldPosition(followPosTransform.position);
				handler.SetParent(followPosTransform);
				handler.Source.clip = Clips.RandomElement();
				handler.Source.PlayOneShot(handler.Source.clip, Settings.Volume.RandomBetweenXY());
				if (Application.isPlaying)
					Destroy(handler.gameObject, Clips[index].length);
				else
					handler.DestroyImmediateOnFinish = true;
			}
        }
		
		public override void PlayOneShot2D()
		{
			if (Clips.IsNullOrEmpty()) return;
			
			var manager = AudioManager.Instance;
            if (manager != null)
            {
	            AudioClip randomClip = Clips.RandomElement();
                //var handler = manager.GetAvailableAudioSourceHandler();
                var handler = manager.GetOneShotAudioSourceHandler(randomClip);
                handler.Setup(in Settings);
				handler.Source.spatialBlend = 0f;
				handler.Source.clip = randomClip;
                handler.Source.PlayOneShot(handler.Source.clip, Settings.Volume.RandomBetweenXY());
            }
			else
			{
				AudioSourceHandler handler = GetFallbackAudioSourceHandler();
			 	int index = ArrangeSourceWithClip(ref handler.Source);
				handler.Source.spatialBlend = 0f;
				handler.Source.PlayOneShot(handler.Source.clip, Settings.Volume.RandomBetweenXY());
				Destroy(handler.gameObject, handler.Source.clip.length);
			}
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
		
		private AudioSourceHandler GetFallbackAudioSourceHandler()
		{
			AudioSourceHandler handler = new GameObject("New Audiosource Handler").AddComponent<AudioSourceHandler>();
			handler.Source = handler.gameObject.AddComponent<AudioSource>();
			return handler;
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
        public override void Stop(bool isEditor = false) { }

        private void OnValidate() => Settings.OnValidate();
    }
}
