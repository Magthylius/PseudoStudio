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

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(Instance);
        }

        private void Start()
        {
            StartCoroutine(HandleObjectPooling());
        }

        public void PlaySFXAt(Transform position) => PlaySFXAt(position.position);

        public void PlaySFXAt(Vector3 position)
        {
            
        }

        #region Object pooling

        IEnumerator HandleObjectPooling()
        {
            for (int i = 0; i < poolingCount; i++)
            {
                GameObject audioSource = Instantiate(AudioObjectPrefab, transform);
                audioSource.SetActive(false);
                yield return new WaitForEndOfFrame();
            }
            
            LoadingManager.Instance.CheckInAudioPool();
            yield return null;
        }

        #endregion
        
    }
}
