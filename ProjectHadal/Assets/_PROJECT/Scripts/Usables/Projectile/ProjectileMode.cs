using UnityEngine;
using NaughtyAttributes;


// C: Jon
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

    [Header("Projectile base settings")]
    [ReadOnly] public ProjectileModeEnum mode;
    public float endTime;
    public bool armsProjectile;
    public bool skipsOnContact;

    protected new Rigidbody rigidbody;
    protected bool frameSetupCompleted = false;

    public abstract void Setup(Rigidbody rb);
    public abstract void FirstFrameSetup();
    public abstract void DoUpdate();
}
