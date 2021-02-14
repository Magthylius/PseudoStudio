using UnityEngine;

[System.Serializable]
public class IdleMode : ProjectileMode
{
    [Header("Idle settings")]
    public bool destroyObject;

    public override void Setup(Rigidbody rb)
    {
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
