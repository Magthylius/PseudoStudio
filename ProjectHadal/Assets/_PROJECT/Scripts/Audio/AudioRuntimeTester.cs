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
        [Header("SFX")]
        [SerializeField] AudioEventData testSfx;
        [Button(nameof(PlaySFX), EButtonEnableMode.Playmode)]
        private void PlaySFX()
        {
            if (testSfx == null) { "Test SFX is null.".Warn(); return; }
            testSfx.Play(listener.transform.position);
        }

        [Space(5)]
        [Header("Ambience")]
        [SerializeField] AudioSource ambSource;
        [SerializeField] AudioEventData testAmb;
        bool isPaused = false;
        [Button(nameof(PlayAmbience), EButtonEnableMode.Playmode)]
        private void PlayAmbience()
        {
            if (testAmb == null || ambSource == null) { "Test AMB or Source is null.".Warn(); return; }
            testAmb.Play(ambSource);
        }
        [Button(nameof(TogglePauseAmbience), EButtonEnableMode.Playmode)]
        private void TogglePauseAmbience()
        {
            if (testAmb == null || ambSource == null) { "Test AMB or Source is null.".Warn(); return; }
            isPaused = !isPaused;
            testAmb.Pause(isPaused);
        }
        [Button(nameof(StopAmbience), EButtonEnableMode.Playmode)]
        private void StopAmbience()
        {
            if (testAmb == null || ambSource == null) { "Test AMB or Source is null.".Warn(); return; }
            testAmb.Stop();
        }
    }
}
