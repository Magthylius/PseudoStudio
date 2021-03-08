using UnityEngine;

// C: Jin
[System.Serializable]
public class AttachMode : ProjectileMode
{
    public override void Setup(Rigidbody rb, Transform rTransform)
    {
        base.Setup(rb, rTransform);
        mode = ProjectileModeEnum.ATTACH;
    }

    public override void FirstFrameSetup()
    {
        frameSetupCompleted = true;
    }

    public override void DoUpdate()
    {
        if (!frameSetupCompleted) FirstFrameSetup();
    }
}
