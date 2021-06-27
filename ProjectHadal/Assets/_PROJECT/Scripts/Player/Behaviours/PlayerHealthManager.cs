using Photon.Pun;
using UnityEngine;
using Hadal.UI;
using Tenshi;
using System;
using System.Collections;
using ExitGames.Client.Photon;
using Hadal.Networking;

//Created by Jet
namespace Hadal.Player.Behaviours
{
    public class PlayerHealthManager : MonoBehaviour, IDamageable, IUnalivable, IKnockable, IInteractable, IPlayerComponent
    {
        [Header("Lifeline Settings")]
        [SerializeField] private bool enableDeathTimerWhenDown;
        [SerializeField, Tooltip("In seconds. Only applicable with death timer is enabled.")] private float deathTime;
        [SerializeField, ReadOnly] private float _deathTimer;

        [Header("Health Settings")]
        [SerializeField] private int maxHealth;
        private int _currentHealth;
        private bool _isDead;
        private bool _isKnocked;
        private float _knockTimer;
        private PhotonView _pView;
        private PlayerController _controller;
        private PlayerCameraController _cameraController;
        public event Action<int> OnHit;
        public event Action OnDeath;
        public event Action OnDown;
        private bool _isKami;
        private bool _initialiseOnce;

        private void Awake()
        {
            _isDead = false;
            _isKami = false;
            ResetHealth();
        }
        private void OnDestroy()
        {
            OnDown = null;
        }
        private void OnValidate()
        {
            if (maxHealth <= 0) maxHealth = 1;
        }

        private void Initialise(PlayerController player)
        {
            if (_controller != player || !_pView.IsMine)
                return;

            PlayerController.OnInitialiseComplete -= Initialise;
            NetworkEventManager.Instance.AddListener(ByteEvents.PLAYER_HEALTH_UPDATE, Receive_HealthUpdate);
        }

        public void DoUpdate(in float deltaTime)
        {
            if (!_isKnocked)
                return;

            if (TickKnockTimer(deltaTime) <= 0f)
                _isKnocked = false;
        }

        public bool TakeDamage(int damage)
        {
            if (_isKami || IsDown || IsUnalive) return false;
            _currentHealth = (_currentHealth - damage).Clamp0();
            DoOnHitEffects(damage);
            CheckHealthStatus();
            Send_HealthUpdateStatus(true);
            return true;
        }

        private void DoOnHitEffects(int damage)
        {
            _cameraController.ShakeCameraDefault();
            OnHit?.Invoke(damage);
            UIManager.InvokeOnHealthChange();
        }

        public void CheckHealthStatus()
        {
            if (IsDown)
            {
                OnDown?.Invoke();
                return;
            }
            if (IsUnalive)
            {
                OnDeath?.Invoke();
                _controller.Die();
            }
        }

        public bool ReviveIfNotDead() => TryRestoreControllerSystem();

        /// <summary>
        /// Resets the current health to its max value. Will refresh the (IsDown == true) function detection:
        /// <see cref="DeactivateControllerSystem"/>
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = maxHealth;
            ResetDeathTimer();
            OnDown += DeactivateControllerSystem;
        }

        /// <summary>
        /// Sets current health value. Should only be used in network callbacks.
        /// Can be used to kill the player, but the hp checking event must be manually triggered after this function.
        /// </summary>
        private void NetOnly_SetHealthValue(in int health)
        {
            _currentHealth = health.Clamp(0, maxHealth);
        }

        /// <summary>
        /// Safely sets current health value. Cannot be used to kill the player, use <see cref="TakeDamage"/> instead.
        /// </summary>
        private void Safe_SetHealthValue(in int health)
        {
            _currentHealth = health.Clamp(1, maxHealth);
        }

        /// <summary>
        /// Safely sets current health to a percentage of max health. Cannot be used to kill the player, use <see cref="TakeDamage"/> instead.
        /// </summary>
        private void Safe_SetHealthToPercent(float percent)
        {
            percent = percent.Clamp01();
            Safe_SetHealthValue((maxHealth * percent).AsInt());
        }

        /// <summary> Set current health to value for debugging purposes. Values below zero are ignored. Will be affected by god mode. </summary>
        public void Debug_SetCurrentHealth(int health)
        {
            _currentHealth = health.Clamp0();
            TakeDamage(0);
        }

        /// <summary> God mode for debugging purposes. Immunity to damage & death. </summary>
        public void Debug_SetGodMode(bool statement) => _isKami = statement;
        public void Debug_ToggleGodMode() => Debug_SetGodMode(!_isKami);

        public void ResetManager()
        {
            ResetHealth();
            UIManager.InvokeOnHealthChange();
            _controller.SetIsDown(false);
        }

        public bool TryToKnock(Vector3 force, float duration)
        {
            if (_isKnocked)
                return false;

            _isKnocked = true;
            ResetKnockTimer(duration);
            _controller.GetInfo.Rigidbody.AddForce(force, ForceMode.Impulse);
            return true;
        }

        public void Interact()
        {
            //! Only players on other machine should be able to send the event to the true networked player of this pView
            if (!_pView.IsMine)
                return;
            
            if (IsDown)
            {

            }
        }

        private float TickKnockTimer(in float deltaTime) => _knockTimer -= deltaTime;
        private void ResetKnockTimer(in float time) => _knockTimer = time;

        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _controller = controller;
            _pView = info.PhotonInfo.PView;
            _cameraController = info.CameraController;

            if (!_initialiseOnce)
            {
                _initialiseOnce = true;
                PlayerController.OnInitialiseComplete += Initialise;
            }
        }

        private void SetRevivalCustomisations()
        {
            Safe_SetHealthToPercent(0.5f); //! Revive at x% hp?
        }

        private void DeactivateControllerSystem()
        {
            OnDown -= DeactivateControllerSystem; //! Unsubscribe so it is only called "once per life"

            _controller.SetIsDown(true); //! Disable movement & rotation
            _controller.SetPhysicHighFriction(); //! Update physics settings

            if (enableDeathTimerWhenDown)
                StartCoroutine(StartDeathTimer());
        }

        private bool TryRestoreControllerSystem()
        {
            if (IsUnalive || !IsDown)
                return false;

            StopAllCoroutines();
            ResetHealth();
            SetRevivalCustomisations();
            _isDead = false; //! Make sure this is false

            _controller.SetIsDown(false); //! Enable movement & rotation
            _controller.SetPhysicNormal(); //! Update physics settings
            return true;
        }

        private IEnumerator StartDeathTimer()
        {
            ResetDeathTimer();
            while (IsDown)
            {
                if (ElapseDeathTimer(_controller.DeltaTime) < 0f)
                {
                    //! Player is officially unalive
                    _isDead = true;
                    CheckHealthStatus();
                    yield break;
                }
                yield return null;
            }
        }

        /// <summary>
        /// Sends a health update event. The boolean, "sendToTrueLocalPlayer", should be properly assigned for the event's flow to work as intended.
        /// </summary>
        /// <param name="sendToTrueLocalPlayer">If true, the nature of the event will be Non-local duplicates -> Local player.
        /// If false, the nature will be Local player -> Non-local duplicates.</param>
        private void Send_HealthUpdateStatus(bool sendToTrueLocalPlayer)
        {
            //! Only the local player should be able to send the event to report to all the simulated networked players
            if (!_pView.IsMine)
                return;
            
            object[] content = new object[]
            {
                _pView.ViewID,
                sendToTrueLocalPlayer,
                _currentHealth,
                _isDead
            };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.PLAYER_HEALTH_UPDATE, content, SendOptions.SendReliable);
        }

        private void Receive_HealthUpdate(EventData data)
        {
            object[] content = data.CustomData.AsObjArray();
            
            int receivedViewID = content[0].AsInt();
            if (_pView.ViewID != receivedViewID)
                return;

            bool sendToTrueLocalPlayer = content[1].AsBool();

            int curHealth = content[2].AsInt();
            NetOnly_SetHealthValue(curHealth);

            bool isDead = content[3].AsBool();
            _isDead = isDead;

            CheckHealthStatus();
        }

        private void ResetDeathTimer() => _deathTimer = deathTime;
        private float ElapseDeathTimer(in float deltaTime) => _deathTimer -= deltaTime;

        public GameObject Obj => gameObject;
        public float GetHealthRatio => _currentHealth / (float)maxHealth;
        public int GetCurrentHealth => _currentHealth;
        public int GetMaxHealth => maxHealth;
        public bool IsUnalive => _isDead;
        public bool IsDown => _currentHealth <= 0;
        public bool IsKnocked => _isKnocked;
    }
}