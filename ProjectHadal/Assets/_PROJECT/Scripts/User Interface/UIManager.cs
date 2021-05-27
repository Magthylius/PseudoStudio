using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hadal.Inputs;
using Magthylius.LerpFunctions;
using Magthylius.Utilities;
using Hadal.Networking;
using Hadal.Networking.UI.Loading;
using Hadal.PostProcess;
using Hadal.Locomotion;

//Created by Jet, Edited by Jon 
namespace Hadal.UI
{
    public delegate void OnHealthChange();
    public delegate void OnPauseMenuAction();

    public enum TrackerType
    {
        PLAYER_NAME = -1,
        SONIC_DART = 0,
    }

    public class UIManager : MonoBehaviourDebug
    {
        public static UIManager Instance;

        public string debugKey;

        NetworkEventManager neManager;
        PostProcessingManager ppManager;
        LoadingManager loadingManager;

        [Header("Essentials")]
        [SerializeField] Camera playerCamera;
        [SerializeField] Canvas overlayCanvas;
        [SerializeField] Canvas cameraCanvas;

        [Header("Reticle Settings")]
        [SerializeField] RectTransform reticleDirectors;
        //[SerializeField] MagthyliusUILineRenderer reticleLineRenderer;
        [SerializeField] float maxDirectorRadius = 10f;
        [SerializeField] float directorReactionSpeed = 5f;
        [SerializeField] float directorInputCamp = 5f;

        [Header("Reticle Line Settings")]
        [SerializeField] Image reticleLineImage;
        [SerializeField] float minPixelsPerUnit;
        [SerializeField] float maxPixelsPerUnit;

        [Header("Reticle Mover Settings")]
        [SerializeField] RectTransform upperMoverGroup;
        [SerializeField] RectTransform lowerMoverGroup;
        [SerializeField, MinMaxSlider(0f, 1f)] Vector2 moverGroupOpacityBounds;
        [SerializeField, MinMaxSlider(0f, 20f)] Vector2 moverGroupPaddingBounds;
        [SerializeField, MinMaxSlider(0f, 20f)] Vector2 moverYVelocityBounds;
        [SerializeField] float moverLerpSpeed;

        CanvasGroupFader umgCGF;
        CanvasGroupFader lmgCGF;
        VerticalLayoutGroup umgVLG;
        VerticalLayoutGroup lmgVLG;

        [Header("Loader Filler Settings")]
        [SerializeField] Image leftLoaderFiller;
        [SerializeField] Image rightLoaderFiller;
        [SerializeField, Range(0f, 1f)] float fillerMinFillClamp = 0.1f;
        [SerializeField, Range(0f, 1f)] float fillerMaxFillClamp = 0.5f;
        [SerializeField] float loaderFillLerpSpeed = 5f;

        FlexibleRect reticleDirectorsFR;

        [Header("Player Settings")]
        [SerializeField] RectTransform allUIParent;
        [SerializeField, Min(0f)] float uiDisplacement;
        [SerializeField, Min(0.1f)] float maxMovementInfluence;
        [SerializeField, Min(0.1f)] float uiLerpReactionSpeed;

        public static event OnHealthChange OnHealthChange;

        FlexibleRect allUIParentFR;

        Rotator playerRotator;
        IRotationInput playerRotationInput;
        Transform playerTransform;
        Rigidbody playerRigidbody;

        [Header("Torpedo Settings")]
        public int torpCount;
        public List<GameObject> tubeIcons;

        public List<Image> floodIndicators;
        public GameObject fireReticle;
        
        public GameObject floodText;

        public List<Image> reloadProgressors;
        public GameObject reloadText;

        [Header("Module Settings")]
        [SerializeField] UITrackerHandler trackerHandler;
        List<Transform> sonicDartTransforms;

        [Header("Utilities Settings")]
        [SerializeField] UIUtilitiesHandler utilitiesHandler;

        [Header("Pause Menu Settings")]
        [SerializeField] Menu pauseMenu;
        StandardUseableInput playerInput;
        public event OnPauseMenuAction PauseMenuOpened;
        public event OnPauseMenuAction PauseMenuClosed;

        bool pauseMenuOpen = false;
        //! blur out the screen

        //! Debug
        int sl_UI;

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                //OnHealthChange += UpdateHealthBar;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            neManager = NetworkEventManager.Instance;
            ppManager = PostProcessingManager.Instance;
            loadingManager = LoadingManager.Instance;

            reticleDirectorsFR = new FlexibleRect(reticleDirectors);
            allUIParentFR = new FlexibleRect(allUIParent);

            //cameraCanvas.worldCamera = playerCamera;

            umgCGF = new CanvasGroupFader(upperMoverGroup.GetComponent<CanvasGroup>(), true, false);
            lmgCGF = new CanvasGroupFader(lowerMoverGroup.GetComponent<CanvasGroup>(), true, false);
            umgVLG = upperMoverGroup.GetComponent<VerticalLayoutGroup>();
            lmgVLG = lowerMoverGroup.GetComponent<VerticalLayoutGroup>();

            DoDebugEnabling(debugKey);

            SetupReticle();
            SetupModules();
            SetupPauseMenu();
            PNTR_Resume();

            //sl_UI = DebugManager.Instance.CreateScreenLogger();
        }

        void Update()
        {
            if (playerTransform == null) return;

#if UNITY_EDITOR
            if (playerInput.TabKeyDown) TriggerPauseMenu();
#else
            if (playerInput.EscKeyDown) TriggerPauseMenu();
#endif
        }

        void FixedUpdate()
        {
            if (playerTransform == null) return;

            if (!pauseMenuOpen)
            {
                UpdateReticle();
                UpdateInformation();
                UpdateProjectileTracking();
                UpdateUIDisplacement();
            }
        }

        //private void OnDestroy() => OnHealthChange -= UpdateHealthBar;
        #endregion

        #region Health
        public static void InvokeOnHealthChange() => OnHealthChange?.Invoke();
        #endregion

        #region Torpedoes
        public void UpdateFlooding(float progress, bool showFlooding)
        {
            /*foreach (Image img in floodIndicators) img.fillAmount = progress;

            if (progress < 1f) fireReticle.SetActive(false);
            else fireReticle.SetActive(true);*/

            float fillProgress = Mathf.Lerp(fillerMinFillClamp, fillerMaxFillClamp, progress);
            leftLoaderFiller.fillAmount = fillProgress;
            rightLoaderFiller.fillAmount = fillProgress;

            floodText.SetActive(showFlooding);

            //DebugLog("Flood Progress: " + progress);
        }

        public void UpdateTubes(int torpedoCount)
        {
            torpCount = torpedoCount;
            //foreach (GameObject tube in tubeIcons) tube.SetActive(false);
            //for (int i = 0; i < torpedoCount - 1; i++) tubeIcons[i].SetActive(true);

            if (torpCount <= 0)
            {
                foreach (GameObject tube in tubeIcons) tube.SetActive(false);
            }
            else
            {
                foreach (GameObject tube in tubeIcons)
                {
                    int result = 0;
                    if (int.TryParse(tube.name, out result))
                    {
                        if (result == torpCount)
                        {
                            tube.SetActive(true);
                            continue;
                        }
                    }
                    tube.SetActive(false);
                }
            }
        }

        public void UpdateReload(float progress, bool showReloading)
        {
            //foreach (Image reloaders in reloadProgressors) reloaders.fillAmount = progress;
            
            reloadText.SetActive(showReloading);
        }
        #endregion

        #region Modules
        public void InjectPlayer(Transform Transform, Rotator Rotator, IRotationInput RotationInput)
        {
            playerTransform = Transform;
            playerRotator = Rotator;
            playerRotationInput = RotationInput;

            playerRigidbody = playerTransform.GetComponent<Rigidbody>();
        }

        void SetupModules()
        {
            //lightsOnString = lightsPrefix + "<color=#" + ColorUtility.ToHtmlStringRGB(lightsOnColor) + ">" + lightsOnSuffix + "</color>";
            //lightsOffString = lightsPrefix + "<color=#" + ColorUtility.ToHtmlStringRGB(lightsOffColor) + ">" + lightsOffSuffix + "</color>";

            sonicDartTransforms = new List<Transform>();
        }

        void UpdateInformation()
        {
            //string depth = Mathf.Abs(Mathf.RoundToInt(highestPoint - playerTransform.position.y)).ToString("#,#");
            //depthText.text = $"Depth: -{depth}";

            if (leftLoaderFiller.fillAmount >= 0.5f) leftLoaderFiller.fillAmount = 0.5f;
            if (rightLoaderFiller.fillAmount >= 0.5f) rightLoaderFiller.fillAmount = 0.5f;
        }

        void UpdateUIDisplacement()
        {
            Vector2 velocity = playerTransform.InverseTransformDirection(playerRigidbody.velocity);
            velocity = -velocity / maxMovementInfluence * uiDisplacement;
            allUIParentFR.StartLerp(velocity);
            allUIParentFR.Step(uiLerpReactionSpeed * Time.deltaTime);
/*
            float velProgress = (Mathf.Abs(velocity.y) - moverYVelocityBounds.x) / moverYVelocityBounds.y;

            if (velocity.y > 0)
            {
                umgCGF.SetAlpha(Mathf.Lerp(moverGroupOpacityBounds.x, moverGroupOpacityBounds.y, velProgress));
                umgVLG.spacing = Mathf.Lerp(moverGroupPaddingBounds.x, moverGroupPaddingBounds.y, velProgress);
            }
            else if (velocity.y < 0)
            {
                lmgCGF.SetAlpha(Mathf.Lerp(moverGroupOpacityBounds.x, moverGroupOpacityBounds.y, velProgress));
                lmgVLG.spacing = Mathf.Lerp(moverGroupPaddingBounds.x, moverGroupPaddingBounds.y, velProgress);
            }*/


           //DebugManager.Instance.SLog(sl_UI, velocity.y);

            /*float upperShaderAlpha = upperMoverImage.material.GetFloat("_Alpha");
            float lowerShaderAlpha = lowerMoverImage.material.GetFloat("_Alpha");

            if (destination.y > moverYVelocityGate) upperMoverImage.material.SetFloat("_Alpha", Mathf.Lerp(upperShaderAlpha, 1f, moverLerpSpeed));
            else upperMoverImage.material.SetFloat("_Alpha", Mathf.Lerp(upperShaderAlpha, 0f, moverLerpSpeed));

            if (destination.y < -moverYVelocityGate) lowerMoverImage.material.SetFloat("_Alpha", Mathf.Lerp(lowerShaderAlpha, 1f, moverLerpSpeed));
            else lowerMoverImage.material.SetFloat("_Alpha", Mathf.Lerp(lowerShaderAlpha, 0f, moverLerpSpeed));*/
        }

        void UpdateProjectileTracking()
        {

        }

        public void TrackProjectile(Transform projectileTransform, TrackerType projectileType)
        {
            switch (projectileType)
            {
                case TrackerType.SONIC_DART:
                    //sonicDartTransforms.Add(projectileTransform);
                    trackerHandler.Scoop(projectileType).TrackTransform(projectileTransform);
                    //if (trackerHandler.Scoop(projectileType) == null) print("balls");
                    break;
            }
        }

        public void UntrackProjectile(Transform projectileTransform)
        {
            trackerHandler.Dump(projectileTransform);
        }

        #endregion

        #region Reticles
        void SetupReticle()
        {
            //! Make 2 points
            List<Vector2> linePoints = new List<Vector2>();
            linePoints.Add(Vector2.zero);
            linePoints.Add(Vector2.zero);

            //reticleLineRenderer.UpdatePoints(linePoints);
        }

        void UpdateReticle()
        {
            Vector2 destination = (Vector2)playerRotationInput.AllInput * maxDirectorRadius;
            if (destination.sqrMagnitude >= maxDirectorRadius * maxDirectorRadius) destination = destination.normalized * maxDirectorRadius;
            reticleDirectorsFR.StartLerp(destination);
            reticleDirectorsFR.Step(directorReactionSpeed * Time.deltaTime);

            float rdFRDist = reticleDirectorsFR.DistanceFromOrigin;
            float linePPU = Mathf.Lerp(minPixelsPerUnit, maxPixelsPerUnit, rdFRDist / maxDirectorRadius);
            reticleLineImage.pixelsPerUnitMultiplier = linePPU;
            reticleLineImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, reticleDirectorsFR.AngleFromOriginDeg);
            reticleLineImage.rectTransform.offsetMax = new Vector2(rdFRDist, reticleLineImage.rectTransform.offsetMax.y);
            //reticleLineRenderer.SetPoint(1, reticleDirectorsFR.center);

            //print(reticleDirectorsFR.AngleFromOrigin);
        }
        #endregion

        #region Utilities
        public void UpdateCurrentUtility(string utilityName)
        {
            utilitiesHandler.UpdateCurrentUtilities(utilityName);
        }
        #endregion

        #region Pause menu
        void SetupPauseMenu()
        {
            playerInput = new StandardUseableInput();
            pauseMenuOpen = false;
            pauseMenu.Close();
        }

        void TriggerPauseMenu()
        {
            pauseMenuOpen = !pauseMenuOpen;

            if (pauseMenuOpen) PNTR_Pause();
            else PNTR_Resume();
        }

        public void PNTR_Debug()
        {
            DebugLog("Pointer entered");
        }

        public void PNTR_Resume()
        {
            pauseMenu.Close();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            //Cursor.lockState = CursorLockMode.Confined;
            if (PauseMenuClosed != null) PauseMenuClosed.Invoke();
        }

        public void PNTR_Pause()
        {
            pauseMenu.Open();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            if (PauseMenuOpened != null) PauseMenuOpened.Invoke();
        }

        public void PNTR_Disconnect()
        {
            neManager.LeaveRoom(true);
        }
        #endregion
    }
}
