using Hadal.Controls;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Magthylius.LerpFunctions;

//Created by Jet
namespace Hadal.UI
{
    using PlayerController = Controls.PlayerController;

    public delegate void OnHealthChange();

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [Header("Position Info")]
        public RectTransform uiRotators;

        [SerializeField] private float highestPoint;
        [SerializeField] private PlayerController player;
        [SerializeField] private Text depthText;
        [SerializeField] private Text lightText;
        [SerializeField] private Image healthBar;
        public static event OnHealthChange OnHealthChange;

        [Header("Shooting Info")]
        public int torpCount;
        public Image floodReticle;
        public List<GameObject> tubeIcons;

        private PlayerLamp _lamp;
        private PlayerHealthManager _healthManager;

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
            //print(player.Rotator.rotation);
        }

        private void OnDestroy() => OnHealthChange -= UpdateHealthBar;

        #region Setup
        public void SetPlayer(PlayerController target)
        {
            player = target;
            _lamp = player.GetComponent<PlayerLamp>();
            _healthManager = player.GetComponent<PlayerHealthManager>();
        }
        #endregion

        #region Player Health
        private void UpdateHealthBar()
        {
            if (player == null) return;
            healthBar.fillAmount = _healthManager.GetHealthRatio;
        }

        public static void InvokeOnHealthChange() => OnHealthChange?.Invoke();
        #endregion

        #region Torpedoes
        public void UpdateFlooding(float progress)
        {
            floodReticle.fillAmount = progress;
        }
        public void UpdateTubes(int torpedoCount)
        {
            torpCount = torpedoCount;
            foreach (GameObject tube in tubeIcons) tube.SetActive(false);
            for (int i = 0; i < torpedoCount - 1; i++)
            {
                tubeIcons[i].SetActive(true);
            }
        }
        #endregion
    }
}