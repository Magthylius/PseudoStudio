using System;
using Hadal.Utility;
using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public abstract class ProjectileObject : MonoBehaviour, IProjectile, IPoolable<ProjectileObject>
    {
        public virtual ProjectileData Data { get; set; }
        public Action<ProjectileObject> DumpEvent { get; set; }
        public Rigidbody Rigidbody { get; private set; }
        public event Action OnHit;
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
            if (NotDamageable(target) || NotTargetLayer(target)) return false;
            DamageTarget(target);
            return true;
        }

        protected virtual void DamageTarget(IDamageable target)
        {
            if (target.TakeDamage(Data.BaseDamage))
            {
                OnHit?.Invoke();
                Dump();
            }

            print($"Hit a {target.Obj.name}");
        }

        private bool NotTargetLayer(IDamageable d) => d.Obj.layer != Data.TargetLayer;
        private static bool NotDamageable(IDamageable d) => d is null;

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

        #endregion

        #region Interface Methods

        public virtual void Dump()
        {
            Data = null;
            DumpEvent?.Invoke(this);
        }

        #endregion
    }
}