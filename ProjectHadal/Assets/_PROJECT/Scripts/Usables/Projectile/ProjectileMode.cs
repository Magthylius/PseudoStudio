using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public abstract class ProjectileMode : MonoBehaviour
{
    public enum ProjectileModeEnum
    {
        PROPULSION = 0,
        IMPULSE,
        IDLE,
        SELF_DESTRUCT,
        SELF_DEACTIVATE
    }

    [ReadOnly] public ProjectileModeEnum mode;
    public float endTime;
    public bool armsProjectile;
    public bool skipsOnContact;

    public abstract void Setup();
}
