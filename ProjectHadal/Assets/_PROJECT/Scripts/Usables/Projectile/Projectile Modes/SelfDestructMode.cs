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

    }

    public override void DoUpdate()
    {

    }
}
