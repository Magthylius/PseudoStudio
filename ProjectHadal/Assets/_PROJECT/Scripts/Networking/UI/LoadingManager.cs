using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Magthylius.LerpFunctions;
using Tenshi.UnitySoku;
using UnityEngine.Events;

namespace Hadal.Networking.UI.Loading
{
    public enum LoadMode
    {
        Press_Any_Key_Continue,
        Load_After_Delay,
        Load_After_Event
    }

    public class LoadingManager : MonoBehaviour
    {
        public static LoadingManager Instance;
        NetworkEventManager neManager;

        [Header("Base settings")]
        [SerializeField] GameObject background;
        [SerializeField] LoadMode loadingMode = LoadMode.Load_After_Delay;
        [SerializeField] float fadeOutDelay = 5f;

        [Header("Animator Settings")]
        public Animator loadingAnimator;
        public float loadingFadeSpeed = 15f;
        public Animator hiveParentAnimator;
        public Animator hiveSpinnerAnimator;
        public Animator connectionAnimator;
        public string connectionAnimatorFinishedBool;

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

        [Header("Loading Checks")]
        [SerializeField] int expectedObjectPoolersCount = 6;

        int objectPoolersCompleted;
        bool objectPoolersCheckedIn;

        public UnityEvent LoadingCompletedEvent;

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        void Start()
        {
            neManager = NetworkEventManager.Instance;

            transform.GetChild(0).gameObject.SetActive(true);

            loadingCG = GetComponent<CanvasGroup>();
            loadingCGF = new CanvasGroupFader(loadingCG, true, true);

            continueCGF = new CanvasGroupFader(continueCG, true, false);
            continueCGF.SetTransparent();

            hiveParentAnimator.gameObject.SetActive(false);
            if (loadingMode == LoadMode.Press_Any_Key_Continue) continueCGF.SetTransparent();

            ResetLoadingElements();

            StopAllAnimators();
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
                    
                    if (loadingMode == LoadMode.Press_Any_Key_Continue)
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
                if (loadingMode == LoadMode.Press_Any_Key_Continue)
                {
                    if (Input.anyKeyDown)
                    {
                        allowContinue = false;
                        FadeOut();
                        loadingCGF.fadeEndedEvent.AddListener(ResetLoadingElements);
                    }
                }
                /*else
                {
                    allowContinue = false;
                    //FadeOut();
                    //loadingCGF.fadeEndedEvent.AddListener(ResetLoadingElements);

                    //loadingCGF.SetTransparent();

                    PlayHiveParent();

                }*/
            }

        }

        #region Load Checks
        IEnumerator CheckAllLoaded()
        {
            while (!objectPoolersCheckedIn)
            {
                yield return null;
            }

            PlayHiveSpinner();
            PlayConnectionParent();

            if (loadingMode == LoadMode.Load_After_Delay)
            {
                StartCoroutine(EndLoading());
            }

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
            background.gameObject.SetActive(true);

            StopAllAnimators();

            continueCGF.SetTransparent();
            loadingCGF.SetTransparent();

            allowLoading = false;
            allowContinue = false;
        }

        IEnumerator EndLoading()
        {
            yield return new WaitForSeconds(fadeOutDelay);

            StopHiveSpinner();
            connectionAnimator.SetTrigger("LoadingReady");

            background.gameObject.SetActive(false);
            hiveSpinnerAnimator.gameObject.SetActive(false);
            PlayHiveParent();

            while (!connectionAnimator.GetBool(connectionAnimatorFinishedBool))
            {
                yield return null;
            }

            connectionAnimator.SetBool(connectionAnimatorFinishedBool, false);
            ResetLoadingElements();

            LoadingCompletedEvent.Invoke();
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
        public void FinishLoading()
        {
            if (loadingMode == LoadMode.Load_After_Event) StartCoroutine(EndLoading());
            else Debug.LogError("Finish Load called, but not set to Load After Event!");
        }

        [Button("Fade In")]
        public void FadeIn()
        {
            loadingCGF.StartFadeIn();
            //Play();
        }
        [Button("Fade Out")]
        public void FadeOut() => loadingCGF.StartFadeOut();

        
        void PlayLoadingAnimator()
        {
            loadingAnimator.Play(0, 0, 0);
            loadingAnimator.speed = 1f;
        }
        void PlayHiveParent()
        {
            hiveParentAnimator.gameObject.SetActive(true);
            hiveParentAnimator.Play(0, 0, 0);
            hiveParentAnimator.speed = 1f;
        }
        void PlayHiveSpinner()
        {
            hiveSpinnerAnimator.Play(0, 0, 0);
            hiveSpinnerAnimator.speed = 1f;
        }
        void PlayConnectionParent()
        {
            connectionAnimator.Play(0, 0, 0);
            connectionAnimator.speed = 1f;
        }

        void StopAllAnimators()
        {
            StopLoadingAnimator();
            StopHiveParent();
            StopHiveSpinner();
            StopConnectionParent();
        }
        void StopLoadingAnimator()
        {
            loadingAnimator.Play(0, 0, 0);
            loadingAnimator.speed = 0f;
        }
        void StopHiveParent()
        {
            hiveParentAnimator.Play(0, 0, 0);
            hiveParentAnimator.speed = 0f;
        }
        void StopHiveSpinner()
        {
            hiveSpinnerAnimator.Play(0, 0, 0);
            hiveSpinnerAnimator.speed = 0f;
        }
        void StopConnectionParent()
        {
            connectionAnimator.Play(0, 0, 0);
            connectionAnimator.speed = 0f;
        }

    }
}

