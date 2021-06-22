using Tenshi.UnitySoku;
using System;
using UnityEngine;
using Hadal.Networking;
using ExitGames.Client.Photon;
using Magthylius.DataFunctions;

//Created by Jet, Edited by Jon
namespace Hadal.Usables.Projectiles
{
    public abstract class ProjectileBehaviour : MonoBehaviourDebug, IProjectile, IPoolable<ProjectileBehaviour>
    {
        protected bool projectileTriggered = false;
        public string DebugKey;
        public int projectileID = 0;
        public bool IsLocal = false;
        public virtual ProjectileData Data { get; set; }
        public virtual ProjectilePhysics PPhysics { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public bool IsArmed { get; set; } = false;
        public bool IsAttached = false;
        public event Action<bool> OnHit;
        public event Action<ProjectileBehaviour> DumpEvent;
        NetworkEventManager neManager;

        [Header("Impact Effect")]
        [SerializeField] protected GameObject particleEffect;
        [SerializeField] protected GameObject projectileAsset;
        protected Timer impactDuration;
        protected bool isVisualizing;
        [SerializeField] protected float impactVFXTime = 5f;

        #region Unity Lifecycle
        protected virtual void Awake() => HandleDependentComponents();
        protected virtual void OnEnable()
        {
            projectileTriggered = false;
        }
        protected virtual void Start()
        {
            DoDebugEnabling(DebugKey);
            neManager = NetworkEventManager.Instance;
            neManager.AddListener(ByteEvents.PROJECTILE_DESPAWN, REdump);
            neManager.AddListener(ByteEvents.PROJECTILE_ATTACH, REattach);
            neManager.AddListener(ByteEvents.PROJECTILE_ACTIVATED, ReTriggerBehavior);
            PPhysics.PhysicsFinished += Dump;
            setIsLocal();
        }
        #endregion

        #region Behavioural Methods

        //For triggering utility (trap) the bool indicates if the utilty is successfully triggered or not.
        public virtual bool TriggerBehavior()
        {
            return false;
        }

        protected virtual void ImpactBehaviour()
        {
            PPhysics.OnPhysicsFinished();
            return;        
        }

        protected virtual void StopImpactEffect()
        {
            isVisualizing = false;
            Rigidbody.isKinematic = false;
            PPhysics.OnPhysicsFinished();
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

        private void setIsLocal()
        {
            if(neManager.isOfflineMode)
            {
                IsLocal = true;
                return;
            }

            for (int i = 0; i < GameManager.Instance.pViewList.Count; i++)
            {
                if (GetShooterID() == GameManager.Instance.pViewList[i].ViewID && GameManager.Instance.pViewList[i].IsMine)
                {
                    IsLocal = true;
                    return;
                }
                else
                {
                    IsLocal = false;
                }
            }
        }

        public void SetPositionRotation(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        #endregion

        #region Event Methods
        protected virtual void REdump(EventData eventData)
        {
            object[] data = (object[])eventData.CustomData;
            if ((int)data[0] == projectileID)
            {
                if(gameObject.activeSelf)
                {
                    gameObject.transform.position = (Vector3)data[1];
                    print(projectileID + "despawning due to event");
                    ImpactBehaviour();
                }
            }
        }

        protected virtual void REattach(EventData eventData)
        {
            object[] data = (object[])eventData.CustomData;
            if ((int)data[0] == projectileID)
            {
                if (gameObject.activeSelf)
                {
                    gameObject.transform.position = (Vector3)data[1];
                    Rigidbody.isKinematic = true;
                    IsAttached = true;
                    print(projectileID + "projectile attaching due to event");

                    if((bool)data[2])
                    {
                        GameObject lev = GameObject.FindGameObjectWithTag("Monster");
                        transform.parent = lev.transform;
                    }
                }
            }
        }

        //For triggering utility (trap), for NON LOCAL players.
        public virtual void ReTriggerBehavior(EventData eventData)
        {
            return;
        }

        protected int GetProjectileTypeID() => Data.ProjTypeInt;

        protected int GetShooterID()
        {
            string ShooterID = projectileID.ToString();

            // return if projectile ID's length is less then 4, I.e., when its not shot by anyone.
            if(ShooterID.Length < 4)
            {
                return 0;
            }

            ShooterID = ShooterID.Substring(0, 4);
            return Convert.ToInt32(ShooterID);
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