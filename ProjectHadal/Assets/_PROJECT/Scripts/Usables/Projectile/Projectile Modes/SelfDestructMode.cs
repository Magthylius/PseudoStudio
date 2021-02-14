using UnityEngine;

[System.Serializable]
public class SelfDestructMode : ProjectileMode
{
    [Header("Destruction settings")]
    public float range;
    public float outwardForce;
    public float inwardForce;

    public bool destroyObject;

    public override void Setup(Rigidbody rb)
    {
        rigidbody = rb;
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
