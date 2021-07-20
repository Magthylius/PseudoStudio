using System;
using System.Collections;
using System.Collections.Generic;
using Hadal.Networking.UI.Loading;
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
            else Destroy(this);
        }

        private void Start()
        {
            StartCoroutine(HandleObjectPooling());
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
            audioSource.SetActive(false);
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
