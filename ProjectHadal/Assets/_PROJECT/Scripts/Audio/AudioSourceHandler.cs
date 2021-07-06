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

        public void DirectPlay()
        {
            Setup();
            Play();
        }
        
        public void Setup()
        {
            //! Need to setup from scriptable
        }
        
        public void Play()
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
        public void Stop()
        {
            Source.Stop();
            AudioStoppedEvent?.Invoke(this);
        }
        public bool IsPlaying => Source.isPlaying;
        
        public bool IsActive => gameObject.activeInHierarchy;
        public void SetActive(bool activeState) => gameObject.SetActive(activeState);
    }
}
