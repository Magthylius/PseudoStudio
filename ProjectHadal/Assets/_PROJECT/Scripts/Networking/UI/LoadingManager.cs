using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Magthylius.LerpFunctions;
using Tenshi.UnitySoku;
using UnityEngine.Events;
using Hadal.PostProcess;
using Hadal.PostProcess.Settings;
using UnityEngine.Rendering.Universal;

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
        PostProcessingManager ppManager;

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

        [Header("Post processing effects")]
        [SerializeField] float postProcessEffectSpeed = 2f;

        public LensDistortionSettings LoadInLensDistortion;
        LensDistortionSettings LoadInLensDistortionEnd;
        LensDistortionSettings currentLensDistortion;

        public ChromaticAberrationSettings LoadInChromaticAberration;
        ChromaticAberrationSettings LoadInChromaticAberrationEnd;
        ChromaticAberrationSettings currentChromaticAberration;

        bool allowPostProcess = false;

        [Header("Events")]
        public UnityEvent LoadingCompletedEvent;

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        void Start()
        {
            neManager = NetworkEventManager.Instance;
            ppManager = PostProcessingManager.Instance;

            transform.GetChild(0).gameObject.SetActive(true);

            loadingCG = GetComponent<CanvasGroup>();
            loadingCGF = new CanvasGroupFader(loadingCG, true, true);

            continueCGF = new CanvasGroupFader(continueCG, true, false);
            continueCGF.SetTransparent();

            //SetupPostProcess();
            ResetLoadingElements();

            LoadingCompletedEvent.AddListener(LoadingCompletedPrint);
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
            }

            if (allowPostProcess)
            {
                float speed = postProcessEffectSpeed * Time.deltaTime;
                float tolerance = 0.0001f;

                ppManager.EditLensDistortion(currentLensDistortion);
                bool a = currentLensDistortion.LerpIntensity(LoadInLensDistortionEnd.Intensity, speed, tolerance);
                bool b = currentLensDistortion.LerpScale(LoadInLensDistortionEnd.Scale, speed, tolerance);

                ppManager.EditChromaticAberration(currentChromaticAberration);
                currentChromaticAberration.LerpIntensity(LoadInChromaticAberrationEnd.Intensity, speed, tolerance);

                if (a && b) allowPostProcess = false;
            }

            //print(connectionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        }

        void LoadingCompletedPrint ()
        {
            print("Loading Completed.");
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
            LoadingCompletedEvent.Invoke();
            if (loadingMode == LoadMode.Load_After_Delay)
            {
                StartEndLoad();
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

        void ActivateLoadingElements()
        {
            loadingCGF.SetOpaque();
            //hiveParentAnimator.gameObject.SetActive(true);
            background.gameObject.SetActive(true);
            //hiveSpinnerAnimator.gameObject.SetActive(true);

            GetCG(hiveParentAnimator.gameObject).alpha = 1f;
            GetCG(hiveSpinnerAnimator.gameObject).alpha = 1f;

            //hiveParentAnimator.enabled = true;
            //hiveSpinnerAnimator.enabled = true;
        }
        void ResetLoadingElements()
        {
            GetCG(hiveParentAnimator.gameObject).alpha = 0f;
            GetCG(hiveSpinnerAnimator.gameObject).alpha = 0f;
            //hiveParentAnimator.enabled = false;
            //hiveSpinnerAnimator.enabled = false;
            //hiveParentAnimator.gameObject.SetActive(false);

            //StopHiveSpinner();
            hiveSpinnerAnimator.SetBool("LoadingReady", false);
            if (loadingMode == LoadMode.Press_Any_Key_Continue) continueCGF.SetTransparent();

            loadingCGF.fadeEndedEvent.RemoveAllListeners();
            background.gameObject.SetActive(true);
            
            StopAllAnimators();

            continueCGF.SetTransparent();
            loadingCGF.SetTransparent();

            allowLoading = false;
            allowContinue = false;
        }

        public void StartEndLoad()
        {
            StartCoroutine(EndLoading());
        }

        IEnumerator EndLoading()
        {
            yield return new WaitForSeconds(fadeOutDelay);

            //StopHiveSpinner();
            connectionAnimator.SetTrigger("LoadingReady");
            hiveSpinnerAnimator.SetBool("LoadingReady", true);

            //yield return null;

            background.gameObject.SetActive(false);
            //hiveSpinnerAnimator.gameObject.SetActive(false);
            GetCG(hiveSpinnerAnimator.gameObject).alpha = 0f;
            hiveSpinnerAnimator.enabled = false;

            PlayHiveParent();

            allowPostProcess = true;

            while (!connectionAnimator.GetBool(connectionAnimatorFinishedBool))
            {
                //print("bool: " + connectionAnimator.GetBool(connectionAnimatorFinishedBool));
                yield return null;
            }

            connectionAnimator.SetBool(connectionAnimatorFinishedBool, false);

            ResetLoadingElements();
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

            SetupPostProcess();
            ActivateLoadingElements();
            SetupPostProcess();
            loadingCGF.fadeEndedEvent.AddListener(ActualLoad);

            nextLoadLevelName = levelName; 
        }
        public void FinishLoading()
        {
            if (loadingMode == LoadMode.Load_After_Event) StartCoroutine(EndLoading());
            else Debug.LogError("Finish Load called, but not set to Load After Event!");
        }

        #region Post processing
        void SetupPostProcess()
        {
            LensDistortion ld;
            if (ppManager.DefaultVolumeTryGet(out ld))
                LoadInLensDistortionEnd = new LensDistortionSettings(ld);

            ChromaticAberration ca;
            if (ppManager.DefaultVolumeTryGet(out ca))
                LoadInChromaticAberrationEnd = new ChromaticAberrationSettings(ca);

            currentLensDistortion = LoadInLensDistortion;
            currentChromaticAberration = LoadInChromaticAberration;
        }
        #endregion

        #region Animators
        [Button("Fade In")]
        public void FadeIn()
        {
            loadingCGF.StartFadeIn();
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
            hiveParentAnimator.enabled = true;
            hiveParentAnimator.Play(0, 0, 0);
            hiveParentAnimator.speed = 1f;
        }
        void PlayHiveSpinner()
        {
            hiveSpinnerAnimator.SetTrigger("AllowLoading");
            hiveSpinnerAnimator.enabled = true;
            hiveSpinnerAnimator.Play(0, 0, 0);
            hiveSpinnerAnimator.speed = 1f;
        }
        void PlayConnectionParent()
        {
            //RuntimeAnimatorController r;
            // connectionAnimator.runtimeAnimatorController = connectionAnimatorController.;

            connectionAnimator.SetTrigger("AllowLoading");
            connectionAnimator.enabled = true;
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
            //connectionAnimator.Play(0, 0, 0);
            //connectionAnimator.speed = 0f;
            connectionAnimator.enabled = false;
        }
        #endregion

        #region Accessors
        CanvasGroup GetCG(GameObject go) => go.GetComponent<CanvasGroup>();
        #endregion
    }
}

