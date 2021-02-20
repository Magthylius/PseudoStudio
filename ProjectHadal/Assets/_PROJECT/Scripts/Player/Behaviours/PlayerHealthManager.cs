using Photon.Pun;
using UnityEngine;

//Created by Jet
namespace Hadal.Player.Behaviours
{
    public class PlayerHealthManager : MonoBehaviour, IDamageable, IPlayerComponent
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

        public GameObject Obj => gameObject;
        public bool TakeDamage(int damage)
        {
            TakeTheDamage(damage);
            return true;
        }
        private void TakeTheDamage(int damage) => _pView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage);

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
            UIManager.InvokeOnHealthChange();
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
            UIManager.InvokeOnHealthChange();
        }

        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _controller = controller;
            _pView = info.PhotonInfo.PView;
            _cameraController = info.CameraController;
        }

        public float GetHealthRatio => _currentHealth / (float)maxHealth;
    }
}