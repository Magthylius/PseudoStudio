using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Magthylius.LerpFunctions;
using Tenshi.UnitySoku;

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
        [SerializeField] bool allowPressAnyKey = true;
        [SerializeField] CanvasGroup continueCG;
        [SerializeField] float continueFadeSpeed = 5f;

        CanvasGroupFader continueCGF;

        AsyncOperation loadingAO;
        string nextLoadLevelName;
        bool allowLoading = false;
        bool allowContinue = false;

        [Header("Loading Checks")]
        [SerializeField] int expectedObjectPoolersCount = 6;

        int objectPoolersCompleted;

        bool objectPoolersCheckedIn;

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
            if (!allowPressAnyKey) continueCGF.SetTransparent();

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
                    
                    if (allowPressAnyKey)
                    {
                        allowContinue = true;
                        continueCGF.StartFadeIn();
                    }
                    else
                    {
                        StartCoroutine(CheckAllLoaded());
                    }

                    loadingCGF.fadeEndedEvent.RemoveAllListeners();
                }
            }


            if (allowContinue)
            {
                if (allowPressAnyKey)
                {
                    if (Input.anyKeyDown)
                    {
                        allowContinue = false;
                        FadeOut();
                        loadingCGF.fadeEndedEvent.AddListener(ResetLoadingElements);
                    }
                }
                else
                {
                    allowContinue = false;
                    FadeOut();
                    loadingCGF.fadeEndedEvent.AddListener(ResetLoadingElements);
                }
            }
        }

        #region Load Checks
        IEnumerator CheckAllLoaded()
        {
            while (!objectPoolersCheckedIn)
            {
                yield return null;
            }

            allowContinue = true;
            yield return null;
        }
        public void CheckInObjectPool()
        {
            if (objectPoolersCheckedIn) return;

            objectPoolersCompleted++;
            if (objectPoolersCompleted >= expectedObjectPoolersCount) objectPoolersCheckedIn = true;
        }
        #endregion


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

