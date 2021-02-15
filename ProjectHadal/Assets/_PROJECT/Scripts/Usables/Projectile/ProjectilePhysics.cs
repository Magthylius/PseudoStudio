using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal;
using NaughtyAttributes;
using Magthylius.DataFunctions;

// C: Jon
public class ProjectilePhysics : MonoBehaviourDebug
{
    public Rigidbody projectileRigidbody;
    [ReadOnly] public List<ProjectileMode> projectileModeList;

    int modeIndex;
    Timer projectileTimer;
    bool allowLaunch;

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

    public void LaunchProjectile()
    {
        allowLaunch = true;
    }

    void SwapModes()
    {
        modeIndex++;
        projectileTimer.SetTickTarget(projectileModeList[modeIndex].endTime);
    }

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
            proj.Setup(projectileRigidbody);
        }
    }
}
