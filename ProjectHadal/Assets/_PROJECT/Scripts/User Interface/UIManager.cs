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

        [Header("Reticle Settings")]
        [SerializeField] RectTransform reticleDirectors;
        [SerializeField] float maxDirectorRadius = 10f;
        [SerializeField] float directorSensitivity = 0.5f;
        [SerializeField] float directorReactionSpeed = 5f;

        //! legacy
        [SerializeField] float rotatorVerticalMovementDistance = 1.2f;
        [SerializeField] float rotatorHorizontalMovementDistance = 0.4f;
        [SerializeField] float rotatorReactionSpeed = 5f;

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

            DoDebugEnabling(debugKey);

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
                InformationUpdate();
                //BalancerUpdate();
                UpdateReticle();
                ProjectileTrackingUpdate();
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

        void InformationUpdate()
        {
            string depth = Mathf.Abs(Mathf.RoundToInt(highestPoint - playerTransform.position.y)).ToString("#,#");
            depthText.text = $"Depth: -{depth}";
        }

        void ProjectileTrackingUpdate()
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
        void UpdateReticle()
        {
            reticleDirectorsFR.StartLerp((Vector2)playerRotationInput.AllInput * maxDirectorRadius);
            reticleDirectorsFR.Step(directorReactionSpeed * Time.deltaTime);

            //print((Vector2)playerRotationInput.AllInput);
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
            //Cursor.lockState = CursorLockMode.Locked;
            Cursor.lockState = CursorLockMode.Confined;
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
