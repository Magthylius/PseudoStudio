using System;
using Hadal.Utility;
using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public abstract class ProjectileObject : MonoBehaviour, IProjectile, IPoolable<ProjectileObject>
    {
        public virtual ProjectileData Data { get; set; }
        public event Action<ProjectileObject> DumpEvent;
        public Rigidbody Rigidbody { get; private set; }
        public event Action<bool> OnHit;
        private Timer _expireTimer;

        #region Unity Lifecycle

        private void Awake()
        {
            HandleDependentComponents();
        }

        protected virtual void Start() => BuildTimer();

        private void OnEnable() => _expireTimer?.Restart();

        private void OnCollisionEnter(Collision collision)
        {
            var damageable = collision.transform.GetComponent<IDamageable>();
            TryDamageTarget(damageable);
        }

        #endregion

        #region Damage/Impact Methods

        protected bool TryDamageTarget(IDamageable target)
        {
            if (NotDamageable(target) || NotTargetLayer(target))
            {
                DisposeOnHit(false);
                return false;
            }
            DamageTarget(target);
            return true;
        }

        protected virtual void DamageTarget(IDamageable target)
        {
            bool didDamage = target.TakeDamage(Data.BaseDamage);
            DisposeOnHit(didDamage);
        }

        private void DisposeOnHit(bool didDamage)
        {
            OnHit?.Invoke(didDamage);
            Dump();
        }
        private bool NotTargetLayer(IDamageable d)
        {
            if (d != null)
            {
                return d.Obj.layer != Data.TargetLayer;
            }
            return false;
        }
        private static bool NotDamageable(IDamageable d) => d == null;

        #endregion

        #region Initialise Methods

        protected virtual void BuildTimer()
        {
            _expireTimer = this.Create_A_Timer()
                        .WithDuration(Data.ExpireTime)
                        .WithOnCompleteEvent(Dump)
                        .WithShouldPersist(true);
        }

        private void HandleDependentComponents()
        {
            Rigidbody = GetComponent<Rigidbody>();
            if (Rigidbody is null)
                Debug.LogWarning($"RigidBody is missing for {name}!");
        }

        public void SetPositionRotation(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        #endregion

        #region Interface Methods

        public virtual void Dump()
        {
            _expireTimer.Pause();
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
            Data = null;
            DumpEvent?.Invoke(this);
            DumpEvent = null;
        }

        #endregion
    }
}