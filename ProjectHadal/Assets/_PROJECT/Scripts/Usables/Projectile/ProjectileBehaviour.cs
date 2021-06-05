using Tenshi.UnitySoku;
using System;
using UnityEngine;

//Created by Jet, Edited by Jon
namespace Hadal.Usables.Projectiles
{
    public abstract class ProjectileBehaviour : MonoBehaviourDebug, IProjectile, IPoolable<ProjectileBehaviour>
    {
        public string DebugKey;
        public int projID = 0;
        public virtual ProjectileData Data { get; set; }
        public virtual ProjectilePhysics PPhysics { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public bool IsArmed { get; set; } = false;
        public bool IsAttached { get; set; } = false;
        public event Action<bool> OnHit;
        public event Action<ProjectileBehaviour> DumpEvent;

        #region Unity Lifecycle

        protected virtual void Awake() => HandleDependentComponents();
        protected virtual void Start()
        {
            DoDebugEnabling(DebugKey);
            PPhysics.PhysicsFinished += Dump;
            projID += Data.ProjIDInt;
        }

        #endregion

        #region Behavioural Methods

        //For triggering utility (trap) the bool indicates if the utilty is successfully triggered or not.
        public virtual bool TriggerBehavior()
        {
            return false;
        }

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

        private void HandleDependentComponents()
        {
            Rigidbody = GetComponent<Rigidbody>();
            PPhysics = GetComponentInChildren<ProjectilePhysics>();
            if (PPhysics != null) PPhysics.SetBehaviour(this);
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