using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Magthylius.LerpFunctions;

namespace Hadal.Networking.UI.Loading
{
    public class LoadingManager : MonoBehaviour
    {
        public static LoadingManager Instance;
        NetworkEventManager neManager;

        [Header("Animator Settings")]
        public Animator loadingAnimator;
        [SerializeField] string loadingStateName;
        public float loadingFadeSpeed = 15f;

        CanvasGroup loadingCG;
        CanvasGroupFader loadingCGF;

        [Header("Continue Settings")]
        [SerializeField] CanvasGroup continueCG;
        [SerializeField] float continueFadeSpeed = 5f;

        CanvasGroupFader continueCGF;

        AsyncOperation loadingAO;
        string nextLoadLevelName;
        bool allowLoading = false;
        bool allowContinue = false;

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        void Start()
        {
            neManager = NetworkEventManager.Instance;

            loadingCG = GetComponent<CanvasGroup>();
            loadingCGF = new CanvasGroupFader(loadingCG, true, true);

            continueCGF = new CanvasGroupFader(continueCG, true, false);

            ResetLoadingElements();
        }

        void FixedUpdate()
        {
            loadingCGF.Step(loadingFadeSpeed * Time.unscaledDeltaTime);
            continueCGF.Step(continueFadeSpeed * Time.unscaledDeltaTime);

            if (allowLoading)
            {
                if (loadingAO != null && loadingAO.isDone)
                {
                    allowLoading = false;
                    continueCGF.StartFadeIn();
                    allowContinue = true;

                    loadingCGF.fadeEndedEvent.RemoveAllListeners();
                }
            }

            if (allowContinue)
            {
                if (Input.anyKeyDown)
                {
                    allowContinue = false;
                    FadeOut();
                    loadingCGF.fadeEndedEvent.AddListener(ResetLoadingElements);
                }
            }
        }

        void ResetLoadingElements()
        {
            loadingCGF.fadeEndedEvent.RemoveAllListeners();

            continueCGF.SetTransparent();
            loadingCGF.SetTransparent();

            allowLoading = false;
            allowContinue = false;
        }

        void ActualLoad()
        {
            allowLoading = true;
            loadingAO = neManager.LoadLevelAsync(nextLoadLevelName);
        }

        /// <summary>
        /// Loads level after transition. Safer to extrapolate level data from NetworkEventManager.
        /// </summary>
        /// <param name="levelName">Name of level.</param>
        public void LoadLevel(string levelName)
        {
            FadeIn();
            loadingCGF.fadeEndedEvent.AddListener(ActualLoad);

            nextLoadLevelName = levelName; 
        }

        [Button("Fade In")]
        public void FadeIn()
        {
            loadingCGF.StartFadeIn();
            Play();
        }
        [Button("Fade Out")]
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

