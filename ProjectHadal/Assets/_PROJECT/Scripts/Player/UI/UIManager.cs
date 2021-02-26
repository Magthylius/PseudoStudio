using Hadal.Player.Behaviours;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hadal.Inputs;
using Magthylius.LerpFunctions;
using Hadal.Networking;
using Hadal.PostProcess;

//Created by Jet
namespace Hadal.Player
{
    public delegate void OnHealthChange();

    public class UIManager : MonoBehaviourDebug
    {
        public static UIManager Instance;

        public string debugKey;

        NetworkEventManager neManager;
        PostProcessingManager ppManager;

        [Header("Position Settings")]
        [SerializeField] RectTransform uiRotators;
        [SerializeField] float rotatorVerticalMovementDistance = 1.2f;
        [SerializeField] float rotatorHorizontalMovementDistance = 0.4f;
        [SerializeField] float rotatorReactionSpeed = 5f;

        FlexibleRect uiRotatorsFR;

        [Header("Player Settings")]
        [SerializeField] private float highestPoint;
        [SerializeField] private PlayerController player;
        [SerializeField] private Text depthText;
        [SerializeField] private Text lightText;
        [SerializeField] private Image reticle;
        [SerializeField] private Image healthBar;
        public static event OnHealthChange OnHealthChange;

        [Header("Shooting Settings")]
        public int torpCount;
        public List<Image> floodIndicators;
        public GameObject fireReticle;
        public List<GameObject> tubeIcons;
        public GameObject floodText;

        public List<Image> reloadProgressors;
        public GameObject reloadText;

        [Header("Module Settings")]
        public TextMeshProUGUI lightsTMP;
        public string lightsPrefix;
        public Color lightsOnColor;
        public Color lightsOffColor;
        public string lightsOnSuffix;
        public string lightsOffSuffix;

        bool isLightOn;
        string lightsOnString;
        string lightsOffString;

        PlayerLamp _lamp;
        PlayerHealthManager _healthManager;

        [Header("Pause Menu Settings")]
        [SerializeField] Menu pauseMenu;
        StandardUseableInput playerInput;

        bool pauseMenuOpen = false;
        //! blur out the screen

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                OnHealthChange += UpdateHealthBar;
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

            DoDebugEnabling(debugKey);

            SetupModules();
            SetupPauseMenu();
        }

        void Update()
        {
            if (player == null) return;

        #if UNITY_EDITOR
            if (playerInput.TabKeyDown) TriggerPauseMenu();
        #else
            if (playerInput.EscKeyDown) TriggerPauseMenu();
        #endif

            if (!pauseMenuOpen)
            {
                InformationUpdate();
                BalancerUpdate();
                SetLights();
            }
        }

        private void OnDestroy() => OnHealthChange -= UpdateHealthBar;
        #endregion

        public void SetPlayer(PlayerController target)
        {
            player = target;
            var info = player.GetInfo;
            _lamp = info.Lamp;
            _healthManager = info.HealthManager;
            SetLights();
        }

        #region Health
        private void UpdateHealthBar()
        {
            if (player == null) return;
            healthBar.fillAmount = _healthManager.GetHealthRatio;
        }

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
        void SetupModules()
        {
            lightsOnString = lightsPrefix + "<color=#" + ColorUtility.ToHtmlStringRGB(lightsOnColor) + ">" + lightsOnSuffix + "</color>";
            lightsOffString = lightsPrefix + "<color=#" + ColorUtility.ToHtmlStringRGB(lightsOffColor) + ">" + lightsOffSuffix + "</color>";

            uiRotatorsFR = new FlexibleRect(uiRotators);
        }

        void InformationUpdate()
        {
            string depth = Mathf.Abs(Mathf.RoundToInt(highestPoint - player.transform.position.y)).ToString("#,#");
            depthText.text = $"Depth: -{depth}";

            string lightIs = _lamp.LightsOn ? "ON" : "OFF";
            lightText.text = $"Light: {lightIs}";
        }

        void BalancerUpdate()
        {
            //float xMovement = player.MovementInput.HorizontalAxis;
            //float yMovement = player.MovementInput.HoverAxis;

            //DebugLog(xMovement + ", " + yMovement);
            //uiRotatorsFR.NormalLerp(uiRotatorsFR.GetBodyOffset(-new Vector2(xMovement * rotatorHorizontalMovementDistance, yMovement * rotatorVerticalMovementDistance)), rotatorReactionSpeed * Time.deltaTime);

            Vector3 balancerAngles = new Vector3();
            balancerAngles.z = player.Rotator.localRotation.eulerAngles.z;

            uiRotators.rotation = Quaternion.Slerp(uiRotators.rotation, Quaternion.Euler(balancerAngles), rotatorReactionSpeed * Time.deltaTime);
            uiRotatorsFR.NormalLerp(uiRotatorsFR.GetBodyOffset(-new Vector2(0f, player.RotationInput.YAxis * rotatorVerticalMovementDistance)), rotatorReactionSpeed * Time.deltaTime);
            //DebugLog(player.transform.localRotation + ", " + player.transform.localRotation.eulerAngles);
        }

        void SetLights()
        {
            if (_lamp.LightsOn) lightsTMP.text = lightsOnString;
            else lightsTMP.text = lightsOffString;
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

            player.Enable();
        }

        public void PNTR_Pause()
        {
            pauseMenu.Open();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            player.Disable();
        }

        public void PNTR_Disconnect()
        {
            neManager.Disconnect();
        }
        #endregion
    }
}
