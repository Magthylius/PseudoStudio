using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal;
using NaughtyAttributes;
using Magthylius.DataFunctions;

// C: Jon, Edited by: Jet, Jin
namespace Hadal.Usables.Projectiles
{
    public class ProjectilePhysics : MonoBehaviourDebug
    {
        public string debugKey;

        [Header("References")]
        public Transform rootTransform;
        public Rigidbody projectileRigidbody;
        public ProjectileBehaviour projectileBehavior;
        [ReadOnly] public List<ProjectileMode> projectileModeList;

        public delegate void PhysicsFinishedEvent();
        public event PhysicsFinishedEvent PhysicsFinished;

        int modeIndex;
        Timer projectileTimer = new Timer(0);
        bool allowLaunch;
        ProjectileBehaviour behaviour;

        void OnValidate()
        {
            SetupProjectileModes();
        }

        void Start()
        {
            SetupProjectileModes();

            modeIndex = 0;

            projectileTimer = new Timer(projectileModeList[0].endTime);
            projectileTimer.TargetTickedEvent.AddListener(SwapModes);

            projectileBehavior = GetComponentInParent<ProjectileBehaviour>();

            DoDebugEnabling(debugKey);
        }

        void Update()
        {
            if (allowLaunch)
            {
                projectileTimer.Tick(Time.deltaTime); 
            }
        }

        void FixedUpdate()
        {
            if (allowLaunch)
            {
                projectileModeList[modeIndex].DoUpdate();
            }    
        }

        //! Trigger projectile launch
        public void LaunchProjectile()
        {
            allowLaunch = true;
        }

        public void SetBehaviour(ProjectileBehaviour behaviour)
        {
            this.behaviour = behaviour;
        }

        //! Finished physics event
        public void OnPhysicsFinished()
        {
            DebugLog("Projectile Physics finished!");

            PhysicsFinished?.Invoke();
            ResetTimer();
        }

        //! Swap modes when one stage is complete
        void SwapModes()
        {
            modeIndex++;
            if (modeIndex >= projectileModeList.Count)
            {
                OnPhysicsFinished();
                allowLaunch = false;
            }
            else
            {
                DebugLog("Projectile Mode: " + projectileModeList[modeIndex].mode.ToString());
                if(projectileModeList[modeIndex].mode == ProjectileMode.ProjectileModeEnum.ATTACH)
                {
                    if(projectileBehavior.IsAttached)
                    {
                        projectileTimer.SetTickTarget(projectileModeList[modeIndex].endTime);
                        return;
                    }
                    else
                    {
                        modeIndex++;
                    }
                }
                projectileTimer.SetTickTarget(projectileModeList[modeIndex].endTime);
            }
        }

        //! Initialization
        void SetupProjectileModes()
        {
            foreach (ProjectileMode proj in projectileModeList)
            {
                if (proj == null)
                {
                    projectileModeList.Remove(proj);
                    SetupProjectileModes();
                    return;
                }
                proj.Setup(projectileRigidbody, rootTransform);
            }
        }

        void ResetTimer()
        {
            modeIndex = 0;
            projectileTimer.Reset();
            projectileTimer.SetTickTarget(projectileModeList[modeIndex].endTime);
        }
    }
}