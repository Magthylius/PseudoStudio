using Tenshi;
using Tenshi.UnitySoku;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using ReadOnly = Tenshi.ReadOnlyAttribute;

namespace Hadal.AudioSystem
{
    public class AudioRuntimeTester : MonoBehaviour
    {
        [SerializeField, ReadOnly] AudioListener listener;
        private void Awake() => listener = FindObjectOfType<AudioListener>();

        [Space(10)]
        [Header("3D SFX")]
        [SerializeField] AudioEventData testSfx;
        [SerializeField] Vector3 offsetFromListener;
        [Button(nameof(PlaySFX), EButtonEnableMode.Playmode)]
        private void PlaySFX()
        {
            if (testSfx == null) { "Test SFX is null.".Warn(); return; }
            testSfx.Play(listener.transform.position + offsetFromListener);
        }

        [Space(10)]
        [Header("Ambience")]
        [SerializeField] AudioEventData testAmb;
        bool isPaused = false;
        [Button(nameof(PlayAmbience), EButtonEnableMode.Playmode)]
        private void PlayAmbience()
        {
            if (testAmb == null) { "Test AMB is null.".Warn(); return; }
            testAmb.Play((AudioSource)null);
        }
        [Button(nameof(TogglePauseAmbience), EButtonEnableMode.Playmode)]
        private void TogglePauseAmbience()
        {
            if (testAmb == null) { "Test AMB is null.".Warn(); return; }
            isPaused = !isPaused;
            testAmb.Pause(isPaused);
        }
        [Button(nameof(StopAmbience), EButtonEnableMode.Playmode)]
        private void StopAmbience()
        {
            if (testAmb == null) { "Test AMB is null.".Warn(); return; }
            testAmb.Stop();
        }
    }
}
