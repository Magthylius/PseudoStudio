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
    protected Transform rootTransform;
    protected bool frameSetupCompleted = false;

    public virtual void Setup(Rigidbody rb, Transform rTransform)
    {
        if (armsProjectile)
        {
            //! do projectile arming
        }

        if (skipsOnContact)
        {

        }

        rootTransform = rTransform;
        rigidbody = rb;
    }

    public abstract void FirstFrameSetup();
    public abstract void DoUpdate();
}
