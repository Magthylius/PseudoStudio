using Hadal.Player.Behaviours;
using UnityEngine;
using UnityEngine.UI;

//Created by Jet
namespace Hadal.Player
{
    public delegate void OnHealthChange();

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        [SerializeField] private float highestPoint;
        [SerializeField] private PlayerController player;
        [SerializeField] private Text depthText;
        [SerializeField] private Text lightText;
        [SerializeField] private Image reticle;
        [SerializeField] private Image healthBar;
        public static event OnHealthChange OnHealthChange;

        private PlayerLamp _lamp;
        private PlayerHealthManager _healthManager;

        public void SetPlayer(PlayerController target)
        {
            player = target;
            _lamp = player.GetComponent<PlayerLamp>();
            _healthManager = player.GetComponent<PlayerHealthManager>();
        }

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
        private void OnDestroy() => OnHealthChange -= UpdateHealthBar;
        private void Update()
        {
            if (player == null) return;
            string depth = Mathf.Abs(Mathf.RoundToInt(highestPoint - player.transform.position.y)).ToString("#,#");
            depthText.text = $"Depth: -{depth}";

            string lightIs = _lamp.LightsOn ? "ON" : "OFF";
            lightText.text = $"Light: {lightIs}";
        }

        private void UpdateHealthBar()
        {
            if (player == null) return;
            healthBar.fillAmount = _healthManager.GetHealthRatio;
        }

        public static void InvokeOnHealthChange() => OnHealthChange?.Invoke();
    }
}