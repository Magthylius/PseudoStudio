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
		[SerializeField] private List<AmbienceGroup> ambiences;

        private bool hasStartedPlaying = false;

        private void OnValidate()
        {
            if (playImmediatelyOnStart)
            {
                playWithDelay = false;
                playDelaySeconds = 0f;
            }
        }

        private void Start() => PlayDefaultAmbience();
		private void OnDestroy()
		{
			GetAmbienceOfType(AmbienceType.Submarine_Engine).Stop();
            GetAmbienceOfType(AmbienceType.Hadal_Zone).Stop();
		}

        public void PlayDefaultAmbience()
        {
            if (hasStartedPlaying) return;
            StartCoroutine(AmbienceRoutine(playWithDelay ? playDelaySeconds : 0f));
        }
		
		public bool PlayAmbienceOfType(AmbienceType type)
		{
			var ambience = GetAmbienceOfType(type);
			if (ambience == null)
				return false;
			
			ambience.Play((AudioSource)null);
			return true;
		}
		
		public bool StopAmbienceOfType(AmbienceType type)
		{
			var ambience = GetAmbienceOfType(type);
			if (ambience == null)
				return false;
			
			ambience.Stop();
			return true;
		}

        private IEnumerator AmbienceRoutine(float delay)
        {
            hasStartedPlaying = true;
            
            if (delay < float.Epsilon)
                yield return null;
            else
                yield return new WaitForSeconds(delay);

			GetAmbienceOfType(AmbienceType.Main_Menu).Stop();
            GetAmbienceOfType(AmbienceType.Submarine_Engine).Play((AudioSource)null);
            GetAmbienceOfType(AmbienceType.Hadal_Zone).Play((AudioSource)null);
        }
		
		private AudioEventData GetAmbienceOfType(AmbienceType type)
		{
			int i = -1;
			while (++i < ambiences.Count)
			{
				if (ambiences[i].associatedEnum == type)
					return ambiences[i].asset;
			}
			return null;
		}
		
		[System.Serializable]
		private class AmbienceGroup
		{
			public AudioEventData asset;
			public AmbienceType associatedEnum;
		}
    }
	
	public enum AmbienceType
	{
		Submarine_Engine = 0,
		Hadal_Zone,
		Hydrophone_Whalesong,
		Grabbed_by_Leviathan,
		Main_Menu,
		AmbushHeartbeat
	}
}