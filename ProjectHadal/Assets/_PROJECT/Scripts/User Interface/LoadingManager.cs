using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Magthylius.LerpFunctions;

namespace Hadal.UI.Loading
{
    public class LoadingManager : MonoBehaviour
    {
        public static LoadingManager Instance;

        [Header("Animator Settings")]
        public Animator loadingAnimator;
        [SerializeField] string loadingStateName;
        public float fadeSpeed = 15f;

        CanvasGroup loadingCG;
        CanvasGroupFader loadingCGF;

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        void Start()
        {
            loadingCG = GetComponent<CanvasGroup>();
            loadingCGF = new CanvasGroupFader(loadingCG, true, true);
        }

        void FixedUpdate()
        {
            loadingCGF.Step(fadeSpeed);
        }

        public void FadeIn() => loadingCGF.StartFadeIn();
        public void FadeOut() => loadingCGF.StartFadeOut();

        [Button("Play Animation")]
        public void Play()
        {
            loadingAnimator.Play(loadingStateName, 0, 0);
            loadingAnimator.speed = 1f;
        }

        [Button("Stop Animation")]
        public void Stop()
        {
            loadingAnimator.speed = 0f;
            loadingAnimator.Play(loadingStateName, 0, 0);
        }

    }
}

