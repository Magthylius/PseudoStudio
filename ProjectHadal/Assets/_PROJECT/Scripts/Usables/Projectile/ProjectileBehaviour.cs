using System;
using Hadal.Utility;
using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public abstract class ProjectileBehaviour : MonoBehaviour, IProjectile, IPoolable<ProjectileBehaviour>
    {
        public virtual ProjectileData Data { get; set; }
        public virtual ProjectilePhysics PPhysics { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public bool IsArmed { get; set; } = false;
        public event Action<bool> OnHit;
        public event Action<ProjectileBehaviour> DumpEvent;
        
        private Timer _expireTimer;

        #region Unity Lifecycle

        protected virtual void Awake() => HandleDependentComponents();
        protected virtual void Start()
        {
            BuildTimer();
            PPhysics.PhysicsFinished += Dump;
        }
        private void OnEnable() => _expireTimer?.Restart();

        #endregion

        #region Behavioural Methods

        public virtual bool ImpactBehaviour(Collision collision)
        {
            var damageable = collision.transform.GetComponent<IDamageable>();
            return TryDamageTarget(damageable);
        }

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
            if (d == null) return false;
            return d.Obj.layer != Data.TargetLayer;
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
            Rigidbody = GetComponentInChildren<Rigidbody>();
            PPhysics = GetComponentInChildren<ProjectilePhysics>();
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
            transform.position = Vector3.zero;

            Data = null;
            DumpEvent?.Invoke(this);
            DumpEvent = null;
        }

        #endregion
    }
}