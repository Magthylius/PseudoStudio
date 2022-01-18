using System;
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
using ExitGames.Client.Photon;
using Hadal.Networking.UI.EndScreen;
using UnityEngine.Serialization;

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
        [SerializeField] GameObject loadingMenuParent;
        [SerializeField] LoadMode loadingMode = LoadMode.Load_After_Delay;
        [SerializeField] float fadeOutDelay = 5f;

        [Header("Animator Settings")]
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
        //[SerializeField] int expectedObjectPoolersCount = 6;

        //int objectPoolersCompleted;
        //bool objectPoolersCheckedIn;

        [SerializeField, ReadOnly] private bool projectilePoolersCheckedIn = false;
        [SerializeField, ReadOnly] private bool audioPoolersCheckedIn = false;

        [FormerlySerializedAs("endsScreenHandler")]
        [Header("End screen")]
        [SerializeField] private EndScreenManager endsScreenManager;

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
        public event Action LoadingFadeEndedEvent;
        bool networkedLoad = false;
        bool allowLoadingCompletion = true;

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        void Start()
        {
            neManager = NetworkEventManager.Instance;
            ppManager = PostProcessingManager.Instance;
            ResetPostProcessing();

            loadingMenuParent.SetActive(true);

            loadingCG = loadingMenuParent.GetComponent<CanvasGroup>();
            loadingCGF = new CanvasGroupFader(loadingCG, true, true);

            continueCGF = new CanvasGroupFader(continueCG, true, false);
            continueCGF.SetTransparent();

            if (!neManager.IsMasterClient)
            {
                neManager.AddListener(ByteEvents.GAME_START_LOAD, NetworkedLoad);
                neManager.AddListener(ByteEvents.GAME_START_END, RE_StartEndScreenAndReturn);
            }

            ResetLoadingElements();

            //LoadingCompletedEvent.AddListener(LoadingCompletedPrint);
            GameManager.Instance.GameEndedEvent += StartEndScreenAndReturn;
        }

        void FixedUpdate()
        {
            //Debug.LogWarning(gameObject.activeInHierarchy);

            loadingCGF.Step(loadingFadeSpeed * Time.unscaledDeltaTime);
            continueCGF.Step(continueFadeSpeed * Time.unscaledDeltaTime);

            if (allowLoading)
            {
                if (loadingAO is { isDone: true } || networkedLoad)
                {
                    allowLoading = false;

                    if (loadingMode == LoadMode.Press_Any_Key_Continue)
                    {
                        allowContinue = true;
                        continueCGF.StartFadeIn();
                    }
                    else
                    {
                        if (GameManager.Instance.LevelHandler.GetCurrentSceneSettings().BypassObjectPoolingChecks)
                        {
                            StartEndLoad();
                            LoadingCompletedEvent.Invoke();

                            PlayHiveSpinner();
                            PlayConnectionParent();
                        }
                        else
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

        }

        void LoadingCompletedPrint()
        {
            Debug.LogWarning("Loading Completed.");
        }

        #region Load Checks
        IEnumerator CheckAllLoaded()
        {
            //! Suspend until all poolers ready
            while (!AllPoolersCheckedIn())
            {
                yield return null;
            }

            PlayHiveSpinner();
            PlayConnectionParent();

            //! Suspend until allowed
            while (!allowLoadingCompletion)
            {
                yield return null;
            }

            LoadingCompletedEvent.Invoke();

            if (loadingMode == LoadMode.Load_After_Delay)
            {
                StartEndLoad();
            }

            yield return null;

            bool AllPoolersCheckedIn() => projectilePoolersCheckedIn && audioPoolersCheckedIn;
        }

        public void CheckInProjectilePool() => projectilePoolersCheckedIn = true;
        public void CheckInAudioPool() => audioPoolersCheckedIn = true;
        #endregion

        void ActivateLoadingElements()
        {
            //loadingCGF.SetOpaque();
            background.gameObject.SetActive(true);

            GetCG(hiveParentAnimator.gameObject).alpha = 1f;
            GetCG(hiveSpinnerAnimator.gameObject).alpha = 1f;

        }
        void ResetLoadingElements()
        {
            GetCG(hiveParentAnimator.gameObject).alpha = 0f;
            GetCG(hiveSpinnerAnimator.gameObject).alpha = 0f;

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
            networkedLoad = false;

            if (loadingMode == LoadMode.Load_After_Event) allowLoadingCompletion = false;
        }

        public void StartEndLoad()
        {
            StartCoroutine(EndLoading());
        }
        IEnumerator EndLoading()
        {
            yield return new WaitForSeconds(fadeOutDelay);
            //Debug.LogWarning($"L check 0");

            connectionAnimator.SetTrigger("LoadingReady");
            hiveSpinnerAnimator.SetBool("LoadingReady", true);

            background.gameObject.SetActive(false);
            //hiveSpinnerAnimator.gameObject.SetActive(false);
            GetCG(hiveSpinnerAnimator.gameObject).alpha = 0f;
            hiveSpinnerAnimator.enabled = false;

            PlayHiveParent();

            allowPostProcess = true;

            //Debug.LogWarning($"L check 1");
            LoadingFadeEndedEvent?.Invoke();
            while (!connectionAnimator.GetBool(connectionAnimatorFinishedBool))
            {
                //print("bool: " + connectionAnimator.GetBool(connectionAnimatorFinishedBool));
                yield return null;
            }
            //Debug.LogWarning($"L check 2");

            connectionAnimator.SetBool(connectionAnimatorFinishedBool, false);

            if (GameManager.Instance.LevelHandler.CurrentScene == GameManager.Instance.InGameScene)
                GameManager.Instance.StartGameEvent();

            ResetLoadingElements();

            //Debug.LogWarning($"L check 3");

        }

        /// <summary>
        /// Load using network event, where the load is done by Photon.
        /// </summary>
        void NetworkedLoad(EventData data)
        {
            //print("recieved network load");
            FadeIn();
            //print("what");
            SetupPostProcess();
            ActivateLoadingElements();

            allowLoading = true;
            networkedLoad = true;
        }

        void ActualLoad()
        {
            //Debug.LogWarning("actual load starting");
            allowLoading = true;
            loadingAO = neManager.LoadLevelAsync(nextLoadLevelName);
            //loadingCGF.FadeInEndedEvent -= ActualLoad;
        }

        /// <summary>
        /// Loads level after transition. Safer to extrapolate level data from NetworkEventManager.
        /// </summary>
        /// <param name="levelName">Name of level.</param>
        public void LoadLevel(string levelName)
        {
            FadeIn();
            //print("what");
            SetupPostProcess();
            ActivateLoadingElements();
            loadingCGF.fadeEndedEvent.AddListener(ActualLoad);
            //loadingCGF.FadeInEndedEvent += ActualLoad;

            //Debug.LogWarning("load the level!");
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

        void ResetPostProcessing()
        {
            if (neManager.isOfflineMode) ppManager.ResetVolumeToDefault();
        }
        #endregion

        #region Animators
        [Button("Fade In")]
        public void FadeIn() => loadingCGF.StartFadeIn();
        [Button("Fade Out")]
        public void FadeOut() => loadingCGF.StartFadeOut();

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
            StopHiveParent();
            StopHiveSpinner();
            StopConnectionParent();
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
            connectionAnimator.enabled = false;
        }
        #endregion

        #region EndScreen

        void StartEndScreenAndReturn(bool playersWon)
        {
            Debug.LogWarning("Game ended! Players Won? " + playersWon);

            object[] data = { playersWon, GameManager.Instance.LevelTimer };
            //Debug.LogWarning(GameManager.Instance.LevelTimer);

            if (NetworkEventManager.Instance.IsMasterClient)
            {
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.GAME_START_END, data);
                Debug.LogWarning("Sending event to end game");
            }

            StartCoroutine(TriggerEndScreen(playersWon, GameManager.Instance.LevelTimer));
        }

        void RE_StartEndScreenAndReturn(EventData data)
        {
            object[] parsedData = (object[])data.CustomData;
            bool playersWon = (bool)parsedData[0];
            float timeTaken = (float)parsedData[1];

            //Debug.LogWarning("Received order to end myself: " + playersWon + ", " + timeTaken);
            StartCoroutine(TriggerEndScreen(playersWon, timeTaken));
        }

        IEnumerator TriggerEndScreen(bool playersWon, float timeTaken)
        {
            yield return new WaitForSeconds(8f);

            //! Enable first before update!
            endsScreenManager.Enable();

            //! have to wait for it to enable
            //while (!endsScreenHandler.IsActive) yield return null;

            endsScreenManager.UpdateEndData(playersWon, timeTaken);
            NetworkEventManager.Instance.LeaveRoom(false, true);
        }

        #endregion

        #region Accessors
        CanvasGroup GetCG(GameObject go) => go.GetComponent<CanvasGroup>();
        public void AllowLoadingCompletion() => allowLoadingCompletion = true;
        #endregion
    }
}

