using UnityEngine;
using Tenshi;
using Hadal.AudioSystem;
using System.Collections;
using Tenshi.UnitySoku;

//Created by Jet
//edited by Jin
namespace Hadal.Player.Behaviours
{
    public class PlayerCollisions : MonoBehaviourDebug, IPlayerComponent
    {
        [Header("Settings")]
        [SerializeField] private bool logCollisionInformation;
        [Space(10)]

        [SerializeField, Min(0f)] private float forceSpeedThreshold;
        [SerializeField] private int collisionDamage;
        [SerializeField] private int collisionDamageMax;
        [SerializeField] private float damageTimer;
        [SerializeField] private AudioEventData collisionSound;

        [Header("Info")]
        [SerializeField, ReadOnly] private Vector3 velocity;
        [SerializeField, ReadOnly] private bool ableToDamage = true;

        private PlayerController _playerController;
        private PlayerCameraController _cameraController;
        private Rigidbody _rBody;

        public Vector3 GetVelocity => velocity;

        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _playerController = controller;
            _cameraController = info.CameraController;
            _rBody = info.Rigidbody;
        }

        internal void DoFixedUpdate(in float fixedDeltaTime)
        {
            if (_rBody == null) return;
            velocity = _rBody.velocity;
        }

        internal void CollisionEnter(Collision collision)
        {
            float force = velocity.magnitude;

            string msg = $"Speed on collision point: {force}\n";

            _cameraController.ShakeCamera(force);

            if (force >= forceSpeedThreshold)
            {
                if (collisionSound)
                    collisionSound.PlayOneShot(_playerController.GetTarget);

                if (!ableToDamage)
                    return;

                float ratio = force / forceSpeedThreshold;
                int damage = collisionDamage * Mathf.RoundToInt(ratio);
                int clampedDamage = Mathf.Clamp(damage, collisionDamage, collisionDamageMax);
                _playerController.GetInfo.HealthManager.TakeDamage(clampedDamage);
                ableToDamage = false;
                StartCoroutine(DamageCDCoroutine(damageTimer));

                msg += $"Damage multiplier (ratio): {ratio}\n";
                msg += $"Raw damage /w multiplier: {damage}\n";
                msg += $"Clamped damage: {clampedDamage}\n";
            }

            if (logCollisionInformation)
                msg.Msg();
        }

        private IEnumerator DamageCDCoroutine(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            ableToDamage = true;
        }

        internal void CollisionStay(Collision collision)
        {

        }

        internal void CollisionExit(Collision collision)
        {
            
        }

        internal void TriggerEnter(Collider collider) { }
        internal void TriggerStay(Collider collider) { }
        internal void TriggerExit(Collider collider) { }
    }
}