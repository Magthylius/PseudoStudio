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

        [Header("Base components")]
        [SerializeField] GameObject background;

        [Header("Overall Settings")]
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

            transform.GetChild(0).gameObject.SetActive(true);

            loadingCG = GetComponent<CanvasGroup>();
            loadingCGF = new CanvasGroupFader(loadingCG, true, true);

            continueCGF = new CanvasGroupFader(continueCG, true, false);
            continueCGF.SetTransparent();

            hiveParentAnimator.gameObject.SetActive(false);
            if (!allowPressAnyKey) continueCGF.SetTransparent();

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

            yield return new WaitForSeconds(fadeOutDelay);

            StopHiveSpinner();
            connectionAnimator.SetTrigger("LoadingReady");

            background.gameObject.SetActive(false);
            hiveSpinnerAnimator.gameObject.SetActive(false);
            PlayHiveParent();
            

            /*while(connectionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f && !connectionAnimator.IsInTransition(0))
            {
                yield return null;
            }

            ResetLoadingElements();*/

            while (!connectionAnimator.GetBool(connectionAnimatorFinishedBool))
            {
                yield return null;
            }

            connectionAnimator.SetBool(connectionAnimatorFinishedBool, false);
            ResetLoadingElements();

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

