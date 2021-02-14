using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal;
using NaughtyAttributes;

public class ProjectilePhysics : MonoBehaviourDebug
{
    [ReadOnly]
    public List<ProjectileMode> projectileModeList;

    void OnValidate()
    {
        SetupProjectileModes();
    }

    void Start()
    {
        SetupProjectileModes();
    }

    void SetupProjectileModes()
    {
        foreach (ProjectileMode proj in projectileModeList) proj.Setup();
    }
}
