﻿using Photon.Pun;
using UnityEngine;
using Hadal.UI;
using Tenshi;
using Tenshi.UnitySoku;
using System;

//Created by Jet
namespace Hadal.Player.Behaviours
{
    public class PlayerHealthManager : MonoBehaviour, IDamageable, IUnalivable, IKnockable, IPlayerComponent
    {
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

        private void Awake()
        {
            _isDead = false;
            _isKami = false;
            ResetHealth();
        }
        private void OnValidate()
        {
            if (maxHealth <= 0) maxHealth += 1;
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
            if (_isKami) return false;
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

        private float TickKnockTimer(in float deltaTime) => _knockTimer -= deltaTime;
        private void ResetKnockTimer(in float time) => _knockTimer = time;

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
        public bool IsKnocked => _isKnocked;
    }
}