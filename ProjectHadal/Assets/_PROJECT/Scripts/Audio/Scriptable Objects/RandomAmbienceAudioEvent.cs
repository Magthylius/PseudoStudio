using ExitGames.Client.Photon;
using Hadal.Networking;
using Photon.Pun;
using Tenshi;
using UnityEngine;

namespace Hadal.AudioSystem
{
    /// <summary> Audio event meant to play random Ambience sounds. </summary>
    [CreateAssetMenu(menuName = "Audio Event/Random Ambience")]
    public class RandomAmbienceAudioEvent : AudioEventData
    {
        [SerializeField] private AudioEventData[] Ambiences;
        [SerializeField] private bool preventConsecutiveRepeatedRandoms = true;
        public override string Description => "Audio event meant to play random Ambience audio events. Audio events put in the list will be called randomly with the play function (refer to examples or ask the Tech for help). "
                                            + "\n\nSupports 2D Random Playing, [Un]Pausing, and Stopping functions.";

        private int chosenIndex = -1;

        public override bool Play(Transform followPosTransform) => false;
        public override bool Play(Vector3 position) => false;

        public override void Play(AudioSource source)
        {
            if (Ambiences.IsNullOrEmpty()) return;

            int index = Random.Range(0, Ambiences.Length);
            if (preventConsecutiveRepeatedRandoms)
                while (chosenIndex == index)
                    index = Random.Range(0, Ambiences.Length);
            
            chosenIndex = index;

            Ambiences[chosenIndex].Play(source);
        }

        public override void Play(int track)
        {
            if (Ambiences.IsNullOrEmpty()) return;
            chosenIndex = track;
            Stop();
            Ambiences[chosenIndex].Play((AudioSource)null);
        }

        public override void Pause(bool isPaused)
        {
            if (Ambiences.IsNullOrEmpty()) return;
            Ambiences[chosenIndex].Pause(isPaused);
        }

        public override void Stop(bool isEditor = false)
        {
            if (Ambiences.IsNullOrEmpty()) return;
            Ambiences[chosenIndex].Stop(isEditor);
        }
    }
}
