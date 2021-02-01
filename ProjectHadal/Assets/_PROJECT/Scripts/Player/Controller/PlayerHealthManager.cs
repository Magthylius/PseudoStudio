﻿using Photon.Pun;
using UnityEngine;

//Created by Jet
namespace Hadal.Controls
{
    public class PlayerHealthManager : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth;
        private int _currentHealth;
        private PhotonView _pView;
        private PlayerController _controller;
        private PlayerCameraController _cameraController;

        private void Awake() => ResetHealth();
        private void OnValidate()
        {
            if (maxHealth == 0) maxHealth += 1;
        }

        public void Inject(PhotonView pView, PlayerController controller, PlayerCameraController cameraControl)
        {
            _pView = pView;
            _controller = controller;
            _cameraController = cameraControl;
        }

        public void TakeDamage(int damage) => _pView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage);

        [PunRPC]
        private void RPC_TakeDamage(int damage)
        {
            if (!_pView.IsMine) return;
            _currentHealth -= damage;
            DoOnHitEffects();
            CheckCurrentHealth();
        }

        private void DoOnHitEffects()
        {
            _cameraController.ShakeCameraDefault();
            UI.UIManager.InvokeOnHealthChange();
        }

        private void CheckCurrentHealth()
        {
            if (_currentHealth > 0) return;
            _controller.Die();
        }
        private void ResetHealth() => _currentHealth = maxHealth;

        public void ResetManager()
        {
            ResetHealth();
            UI.UIManager.InvokeOnHealthChange();
        }
        public float GetHealthRatio => _currentHealth / maxHealth.AsFloat();
    }
}