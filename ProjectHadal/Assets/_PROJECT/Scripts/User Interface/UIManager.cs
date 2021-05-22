using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hadal.Inputs;
using Magthylius.LerpFunctions;
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

        FlexibleRect reticleDirectorsFR;

        [Header("Player Settings")]
        [SerializeField] private float highestPoint;  
        [SerializeField] private Text depthText;
        [SerializeField] private Text lightText;
        [SerializeField] private Image reticle;
        [SerializeField] private Image healthBar;
        public static event OnHealthChange OnHealthChange;

        Rotator playerRotator;
        IRotationInput playerRotationInput;
        Transform playerTransform;

        [Header("Shooting Settings")]
        public int torpCount;
        public List<Image> floodIndicators;
        public GameObject fireReticle;
        public List<GameObject> tubeIcons;
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

            cameraCanvas.worldCamera = playerCamera;

            DoDebugEnabling(debugKey);

            SetupReticle();
            SetupModules();
            SetupPauseMenu();
            PNTR_Resume();
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
            foreach (Image img in floodIndicators) img.fillAmount = progress;

            if (progress < 1f) fireReticle.SetActive(false);
            else fireReticle.SetActive(true);

            floodText.SetActive(showFlooding);

            //DebugLog("Flood Progress: " + progress);
        }

        public void UpdateTubes(int torpedoCount)
        {
            torpCount = torpedoCount;
            foreach (GameObject tube in tubeIcons) tube.SetActive(false);
            for (int i = 0; i < torpedoCount - 1; i++) tubeIcons[i].SetActive(true);
        }

        public void UpdateReload(float progress, bool showReloading)
        {
            foreach (Image reloaders in reloadProgressors) reloaders.fillAmount = progress;
            reloadText.SetActive(showReloading);
        }
        #endregion

        #region Modules
        public void InjectPlayer(Transform Transform, Rotator Rotator, IRotationInput RotationInput)
        {
            playerTransform = Transform;
            playerRotator = Rotator;
            playerRotationInput = RotationInput;
        }

        void SetupModules()
        {
            //lightsOnString = lightsPrefix + "<color=#" + ColorUtility.ToHtmlStringRGB(lightsOnColor) + ">" + lightsOnSuffix + "</color>";
            //lightsOffString = lightsPrefix + "<color=#" + ColorUtility.ToHtmlStringRGB(lightsOffColor) + ">" + lightsOffSuffix + "</color>";

            sonicDartTransforms = new List<Transform>();
        }

        void UpdateInformation()
        {
            string depth = Mathf.Abs(Mathf.RoundToInt(highestPoint - playerTransform.position.y)).ToString("#,#");
            depthText.text = $"Depth: -{depth}";
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
