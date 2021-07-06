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

        public event AudioEvent AudioFinishedEvent;
        public event AudioEvent AudioStoppedEvent;

        private void OnEnable()
        {
            if (!Source) Source = GetComponent<AudioSource>();
        }

        /// <summary> Sets position of this handler's game object. </summary>
        public void SetWorldPosition(Vector3 position) => transform.position = position;

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
                while (IsPlaying) yield return null;
                AudioFinishedEvent?.Invoke(this);
            }
        }
        public void PlayOneShot(AudioClip clip) => Source.PlayOneShot(clip);
        public void Pause() => Source.Pause();
        public void Stop() => Source.Stop();
        public bool IsPlaying => Source.isPlaying;

        public bool IsActive => gameObject.activeInHierarchy;
        public void SetActive(bool activeState) => gameObject.SetActive(activeState);
    }
}
