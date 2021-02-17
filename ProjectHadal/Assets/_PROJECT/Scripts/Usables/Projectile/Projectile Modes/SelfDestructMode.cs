using UnityEngine;

// C: Jon
[System.Serializable]
public class SelfDestructMode : ProjectileMode
{
    [Header("Destruction settings")]
    public float range;
    public float outwardForce;
    public float inwardForce;

    public bool destroyObject;

    public override void Setup(Rigidbody rb, Transform rTransform)
    {
        base.Setup(rb, rTransform);
        mode = ProjectileModeEnum.SELF_DESTRUCT;
    }

    public override void FirstFrameSetup()
    {
        frameSetupCompleted = false;

        rigidbody.AddExplosionForce(outwardForce, rigidbody.position, range);
    }

    public override void DoUpdate()
    {
        if (!frameSetupCompleted) FirstFrameSetup();
    }
}
