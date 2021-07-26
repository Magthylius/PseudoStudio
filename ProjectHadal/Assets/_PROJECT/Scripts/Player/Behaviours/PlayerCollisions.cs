using UnityEngine;
using Hadal.Locomotion;
using Tenshi;
using Hadal.AudioSystem;
using System.Collections;

//Created by Jet
//edited by Jin
namespace Hadal.Player.Behaviours
{
    public class PlayerCollisions : MonoBehaviourDebug, IPlayerComponent
    {
        [Header("Layer Collisions")]
        [SerializeField, Min(0f)] private float forceSpeedThreshold;

        private PlayerController _playerController;
        private PlayerCameraController _cameraController;
        [SerializeField] private int collisionDamage;
        [SerializeField] private int collisionDamageMax;
        [SerializeField] private float damageTimer;
        [SerializeField] private bool ableToDamage = true;

        [SerializeField] private AudioEventData collisionSound;

        private IEnumerator damageCDCoroutine;
        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _playerController = controller;
            _cameraController = info.CameraController;
        }

        internal void CollisionEnter(Collision collision)
        {
            float force = _playerController.GetInfo.Rigidbody.velocity.magnitude;
            
            if (force >= forceSpeedThreshold)
            {
                _cameraController.ShakeCamera(force);

                if (collisionSound)
                    collisionSound.PlayOneShot(_playerController.GetTarget);

                if (!ableToDamage)
                    return;

                float ratio = (force / forceSpeedThreshold);
                int damage = collisionDamage * Mathf.RoundToInt(ratio);
                /*Debug.LogError(damage + " before clamp");*/
                damage = Mathf.Clamp(damage, collisionDamage, collisionDamageMax);
                /*Debug.LogError(damage + " after clamp");*/
                _playerController.GetInfo.HealthManager.TakeDamage(damage);
                ableToDamage = false;
                damageCDCoroutine = DamageCDCoroutine(damageTimer);
                StartCoroutine(damageCDCoroutine);
            }
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