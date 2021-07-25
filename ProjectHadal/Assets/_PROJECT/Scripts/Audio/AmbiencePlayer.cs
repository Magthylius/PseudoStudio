using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Hadal.AudioSystem
{
    public class AmbiencePlayer : MonoBehaviour
    {
        [SerializeField] private bool playImmediatelyOnStart;
        [SerializeField] private bool playWithDelay;
        [SerializeField, Min(0f)] private float playDelaySeconds;
        [SerializeField] private AudioEventData ambienceAsset;
		[SerializeField] private AudioEventData hydrophoneAmbienceAsset;

        private bool hasStartedPlaying = false;

        private void OnValidate()
        {
            if (playImmediatelyOnStart)
            {
                playWithDelay = false;
                playDelaySeconds = 0f;
            }
        }

        private void Start() => StartAmbience();

        public void StartAmbience()
        {
            if (ambienceAsset == null || hasStartedPlaying) return;
            StartCoroutine(AmbienceRoutine(playWithDelay ? playDelaySeconds : 0f));
        }
		
		public void PlayHydrophoneAmbience()
		{
			if (hydrophoneAmbienceAsset != null)
				hydrophoneAmbienceAsset.Play((AudioSource)null);
		}

        private IEnumerator AmbienceRoutine(float delay)
        {
            hasStartedPlaying = true;
            
            if (delay < float.Epsilon)
                yield return null;
            else
                yield return new WaitForSeconds(delay);

            ambienceAsset.Play((AudioSource)null);
        }

        public AudioEventData AmbienceEvent => ambienceAsset;
    }
}