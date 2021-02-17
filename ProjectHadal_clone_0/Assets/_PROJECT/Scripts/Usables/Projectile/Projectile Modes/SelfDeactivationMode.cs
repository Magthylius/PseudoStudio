using UnityEngine;

// C: Jon
[System.Serializable]
public class SelfDeactivationMode : ProjectileMode
{
    [Header("Deactivation settings")]
    public bool destroyObject;

    public override void Setup(Rigidbody rb, Transform rTransform)
    {
        base.Setup(rb, rTransform);
        mode = ProjectileModeEnum.SELF_DEACTIVATE;
    }

    public override void FirstFrameSetup()
    {

    }

    public override void DoUpdate()
    {
        
    }
}
