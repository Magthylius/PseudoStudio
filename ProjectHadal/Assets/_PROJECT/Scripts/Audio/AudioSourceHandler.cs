using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AudioSystem
{
    public delegate void AudioEvent(AudioSourceHandler handler);

    public class AudioSourceHandler : MonoBehaviour
    {
        public AudioSource Source;
        public bool DumpOnFinish = true;
        public Transform OriginalParent;

        public event AudioEvent AudioFinishedEvent;
        public event AudioEvent AudioStoppedEvent;

        private void OnEnable()
        {
            if (!Source) RefreshSource();
        }

        /// <summary> Sets position of this handler's game object. </summary>
        public void SetWorldPosition(Vector3 position) => transform.position = position;
        public void SetParent(Transform parent)
        {
            if (parent == null)
            {
                transform.parent = OriginalParent;
                return;
            }
            transform.parent = parent;
        }

        /// <summary> Called by any relevant scriptable object to configure the audio source attached to this monobehaviour. </summary>
        public void Setup(in AudioSourceSettings settings) => settings.AssignSettings(ref Source);

        /// <summary>
        /// Plays the attached audio source (preferably after calling Setup()) and starts the audio finish timer coroutine.
        /// </summary>
        public void PlaySource()
        {
            Source.Play();
            StartCoroutine(CheckAudioFinished());

            IEnumerator CheckAudioFinished()
            {
                yield return null; //! skip frame to make sure audio source is registered as playing
                
                while (IsPlaying)
                    yield return null;
                
                transform.parent = OriginalParent != null ? OriginalParent : null;
                AudioFinishedEvent?.Invoke(this);
            }
        }
        public void Pause() => Source.Pause();
        public void UnPause() => Source.UnPause();
        public void Stop()
        {
            Source.Stop();
            AudioStoppedEvent?.Invoke(this);
        }
        public bool IsPlaying => Source.isPlaying;

        public bool IsActive => gameObject.activeInHierarchy;
        public void SetActive(bool activeState) => gameObject.SetActive(activeState);
        public void RefreshSource() => Source = GetComponent<AudioSource>();
    }
}
