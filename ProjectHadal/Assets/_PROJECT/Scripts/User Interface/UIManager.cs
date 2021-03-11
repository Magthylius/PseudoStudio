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

// Required Event 
// TODO PLAYER DISABLE/ENABLE EVENT 

//Created by Jet, Edited by Jon 
namespace Hadal.UI
{
    public delegate void OnHealthChange();
    public delegate void OnPauseMenuAction();

    public enum TrackerType
    {
        SONIC_DART = 0,
    }

    public class UIManager : MonoBehaviourDebug
    {
        public static UIManager Instance;

        public string debugKey;

        NetworkEventManager neManager;
        PostProcessingManager ppManager;
        LoadingManager loadingManager;

        [Header("Position Settings")]
        [SerializeField] RectTransform uiRotators;
        [SerializeField] float rotatorVerticalMovementDistance = 1.2f;
        [SerializeField] float rotatorHorizontalMovementDistance = 0.4f;
        [SerializeField] float rotatorReactionSpeed = 5f;

        FlexibleRect uiRotatorsFR;

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
        UITrackerHandler trackerHandler;
        List<Transform> sonicDartTransforms;

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

            DoDebugEnabling(debugKey);

            SetupModules();
            SetupPauseMenu();
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
                BalancerUpdate();
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

            uiRotatorsFR = new FlexibleRect(uiRotators);

            sonicDartTransforms = new List<Transform>();
        }

        void InformationUpdate()
        {
            string depth = Mathf.Abs(Mathf.RoundToInt(highestPoint - playerTransform.position.y)).ToString("#,#");
            depthText.text = $"Depth: -{depth}";
        }

        void BalancerUpdate()
        {
            //float xMovement = player.MovementInput.HorizontalAxis;
            //float yMovement = player.MovementInput.HoverAxis;

            //DebugLog(xMovement + ", " + yMovement);
            //uiRotatorsFR.NormalLerp(uiRotatorsFR.GetBodyOffset(-new Vector2(xMovement * rotatorHorizontalMovementDistance, yMovement * rotatorVerticalMovementDistance)), rotatorReactionSpeed * Time.deltaTime);

            Vector3 balancerAngles = new Vector3();
            balancerAngles.z = playerRotator.localRotation.eulerAngles.z; 

            uiRotators.rotation = Quaternion.Slerp(uiRotators.rotation, Quaternion.Euler(balancerAngles), rotatorReactionSpeed * Time.deltaTime);
            uiRotatorsFR.NormalLerp(uiRotatorsFR.GetBodyOffset(-new Vector2(0f, playerRotationInput.YAxis * rotatorVerticalMovementDistance)), rotatorReactionSpeed * Time.deltaTime);
            //DebugLog(player.transform.localRotation + ", " + player.transform.localRotation.eulerAngles);
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
                    break;
            }
        }

        public void UntrackProjectile(Transform projectileTransform, TrackerType projectileType)
        {
            switch (projectileType)
            {
                case TrackerType.SONIC_DART:
                    foreach (Transform sonicDart in sonicDartTransforms)
                        if (sonicDart == projectileTransform) sonicDartTransforms.Remove(sonicDart);
                    break;
            }
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
