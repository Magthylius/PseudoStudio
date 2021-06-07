using Photon.Pun;
using UnityEngine;
using Hadal.UI;
using Tenshi;
using Tenshi.UnitySoku;
using System;

//Created by Jet
namespace Hadal.Player.Behaviours
{
    public class PlayerHealthManager : MonoBehaviour, IDamageable, IUnalivable, IPlayerComponent
    {
        [SerializeField] private int maxHealth;
        private int _currentHealth;
        private bool _isDead;
        private PhotonView _pView;
        private PlayerController _controller;
        private PlayerCameraController _cameraController;
        public event Action<int> OnHit;
        public event Action OnDeath;
        public event Action OnDown;

        private void Awake()
        {
            _isDead = false;
            ResetHealth();
        }
        private void OnValidate()
        {
            if (maxHealth <= 0) maxHealth += 1;
        }

        public bool TakeDamage(int damage)
        {
            _currentHealth = (_currentHealth - damage).Clamp0();
            DoOnHitEffects(damage);
            CheckHealthStatus();
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
        public void ResetHealth() => _currentHealth = maxHealth;

        public void ResetManager()
        {
            ResetHealth();
            UIManager.InvokeOnHealthChange();
        }

        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _controller = controller;
            _pView = info.PhotonInfo.PView;
            _cameraController = info.CameraController;
        }

        public GameObject Obj => gameObject;
        public float GetHealthRatio => _currentHealth / (float)maxHealth;
        public int GetCurrentHealth => _currentHealth;
        public int GetMaxHealth => maxHealth;
        public bool IsUnalive => _isDead;
        public bool IsDown => _currentHealth <= 0;
    }
}