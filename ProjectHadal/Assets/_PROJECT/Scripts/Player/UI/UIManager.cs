using Hadal.Player.Behaviours;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Created by Jet
namespace Hadal.Player
{
    public delegate void OnHealthChange();

    public class UIManager : MonoBehaviourDebug
    {
        public static UIManager Instance;

        public string debugKey;

        [Header("Position Info")]
        public RectTransform uiRotators;

        [SerializeField] private float highestPoint;
        [SerializeField] private PlayerController player;
        [SerializeField] private Text depthText;
        [SerializeField] private Text lightText;
        [SerializeField] private Image reticle;
        [SerializeField] private Image healthBar;
        public static event OnHealthChange OnHealthChange;

        [Header("Shooting Info")]
        public int torpCount;
        public List<Image> floodIndicators;
        public GameObject fireReticle;
        public List<GameObject> tubeIcons;
        public GameObject floodText;

        public List<Image> reloadProgressors;
        public GameObject reloadText;

        [Header("Module Info")]
        public TextMeshProUGUI lightsTMP;
        public string lightsPrefix;
        public Color lightsOnColor;
        public Color lightsOffColor;
        public string lightsOnSuffix;
        public string lightsOffSuffix;

        bool isLightOn;
        string lightsOnString;
        string lightsOffString;

        private PlayerLamp _lamp;
        private PlayerHealthManager _healthManager;

        #region Unity Lifecycle
        private void Awake()
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

        private void Start()
        {
            DoDebugEnabling(debugKey);

            lightsOnString = lightsPrefix + "<color=#" + ColorUtility.ToHtmlStringRGB(lightsOnColor) + ">" + lightsOnSuffix + "</color>";
            lightsOffString = lightsPrefix + "<color=#" + ColorUtility.ToHtmlStringRGB(lightsOffColor) + ">" + lightsOffSuffix + "</color>";

            ToggleLights();
        }

        private void Update()
        {
            if (player == null) return;
            string depth = Mathf.Abs(Mathf.RoundToInt(highestPoint - player.transform.position.y)).ToString("#,#");
            depthText.text = $"Depth: -{depth}";

            string lightIs = _lamp.LightsOn ? "ON" : "OFF";
            lightText.text = $"Light: {lightIs}";

            Quaternion rotatorAngles = Quaternion.identity;
            rotatorAngles.z = player.Rotator.rotation.z;
            rotatorAngles.w = player.Rotator.rotation.w;

            uiRotators.rotation = rotatorAngles;

            if (Input.GetKeyDown(KeyCode.J)) ToggleLights();
        }

        private void OnDestroy() => OnHealthChange -= UpdateHealthBar;
        #endregion


        public void SetPlayer(PlayerController target)
        {
            player = target;
            _lamp = player.GetComponent<PlayerLamp>();
            _healthManager = player.GetComponent<PlayerHealthManager>();
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

            DebugLog("Flood Progress: " + progress);
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
        void ToggleLights()
        {
            isLightOn = !isLightOn;

            if (isLightOn) lightsTMP.text = lightsOnString;
            else lightsTMP.text = lightsOffString;
        }
        #endregion
    }
}
