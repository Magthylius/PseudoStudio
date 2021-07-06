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
            else Destroy(Instance);
        }

        private void Start()
        {
            StartCoroutine(HandleObjectPooling());
        }

        public void PlayAudioAt(Transform audioTransform) => PlayAudioAt(audioTransform.position);

        public void PlayAudioAt(Vector3 position)
        {
            
        }

        #region Object pooling
        IEnumerator HandleObjectPooling()
        {
            for (int i = 0; i < poolingCount; i++)
            {
                InstantiateAudioSource();
                yield return new WaitForEndOfFrame();
            }
            
            LoadingManager.Instance.CheckInAudioPool();
            yield return null;
        }

        void InstantiateAudioSource()
        {
            GameObject audioSource = Instantiate(AudioObjectPrefab, transform);
            audioSource.SetActive(false);
            audioSourceHandlers.Add(audioSource.GetComponent<AudioSourceHandler>());
        }
        
        AudioSourceHandler Scoop()
        {
            foreach (AudioSourceHandler handler in audioSourceHandlers)
            {
                //if (handler)
            }

            return null;
        }
        #endregion
        
    }
}
