using System;
using System.Collections;
using System.Collections.Generic;
using Hadal.Networking.UI.Loading;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Hadal.AudioSystem
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Audio source pooling")]
        public GameObject AudioObjectPrefab;
        [SerializeField] private int poolingCount = 10;

        private List<AudioSourceHandler> audioSourceHandlers = new List<AudioSourceHandler>();

        void Awake()
        {
            if (Instance == null) Instance = this;
            else
			{
				Destroy(this);
				return;
			}
			
			SceneManager.sceneLoaded += HandleInitialise;
        }
		
		private void OnDestroy()
		{
			SceneManager.sceneLoaded -= HandleInitialise;
			try
			{
				SceneManager.sceneUnloaded -= HandleDeinitialise;
			}
			catch { }
		}
		
		private void HandleInitialise(Scene scene, LoadSceneMode mode)
		{
			//! Only initialise for in-game scene
			if (scene.buildIndex != 2)
				return;
			
			//! Subscribe once per load
			SceneManager.sceneUnloaded += HandleDeinitialise;
			
			//! Initialise object pool
			StartCoroutine(HandleObjectPooling());
		}
		
		private void HandleDeinitialise(Scene current)
		{
			//! Only deinitialise for in-game scene
			if (current.buildIndex != 2)
				return;
			
			//! Unsubscribe once per unload
			SceneManager.sceneUnloaded -= HandleDeinitialise;
			StopAllCoroutines();
			
			//! Stop all audio
			int i = -1;
			while (++i < audioSourceHandlers.Count)
			{
				if (audioSourceHandlers[i] != null)
					audioSourceHandlers[i].Stop();
			}
			
			//! Destroy all audio objects
			for (i = audioSourceHandlers.Count - 1; i >= 0; i--)
			{
				var handler = audioSourceHandlers[i];
				audioSourceHandlers.RemoveAt(i);
				if (handler != null)
					Destroy(handler.gameObject);
			}
		}

        /// <summary>
        /// Returns an available audio source handler from the object pool, setting it up with the dump event and activating its game object.
        /// </summary>
        public AudioSourceHandler GetAvailableAudioSourceHandler()
        {
            AudioSourceHandler handler = Scoop();
            handler.SetActive(true);
            if (handler.DumpOnFinish) handler.AudioFinishedEvent += Dump;
            return handler;
        }

        public AudioSourceHandler GetOneShotAudioSourceHandler(AudioClip clip)
        {
	        foreach (AudioSourceHandler handler in audioSourceHandlers)
	        {
                if (handler == null || handler.Source == null)
                    continue;
                
		        if (handler.Source.clip == clip)
		        {
			        return handler;
		        }
	        }
	        
	        return InstantiateAudioSource();
        }

        #region Object pooling
        IEnumerator HandleObjectPooling()
        {
            for (int i = 0; i < poolingCount; i++)
            {
                InstantiateAudioSource();
                yield return null;
            }
            
            LoadingManager.Instance.CheckInAudioPool();
            yield return null;
        }

        AudioSourceHandler InstantiateAudioSource()
        {
            GameObject audioSource = Instantiate(AudioObjectPrefab, transform);
            audioSource.SetActive(true);
            AudioSourceHandler handler = audioSource.GetComponent<AudioSourceHandler>();
            handler.OriginalParent = transform;
            audioSourceHandlers.Add(handler);
            return handler;
        }
        
        /// <summary> Gets an inactive audio source in pool. </summary>
        AudioSourceHandler Scoop()
        {
            foreach (AudioSourceHandler handler in audioSourceHandlers)
            {
				if (handler == null)
					continue;
				
                if (!handler.IsActive)
                    return handler;
            }

            //! Cannot find any inactive sources
            Debug.LogWarning("No inactive sources found, instantiating new source.");
            return InstantiateAudioSource();
        }

        /// <summary> Unsubscribes this callback from the audio finished event of a handler and deactivate it. </summary>
        private void Dump(AudioSourceHandler handler)
        {
            handler.AudioFinishedEvent -= Dump;
            handler.SetActive(false);
        }
        
        #endregion
        
    }
}
