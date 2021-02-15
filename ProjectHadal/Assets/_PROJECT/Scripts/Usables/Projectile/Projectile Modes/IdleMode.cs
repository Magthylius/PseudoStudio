using UnityEngine;

// C: Jon
[System.Serializable]
public class IdleMode : ProjectileMode
{
    [Header("Idle settings")]
    public bool destroyObject;

    public override void Setup(Rigidbody rb, Transform rTransform)
    {
        rootTransform = rTransform;
        rigidbody = rb;
        mode = ProjectileModeEnum.IDLE;
    }

    public override void FirstFrameSetup()
    {

    }

    public override void DoUpdate()
    {

    }
}
