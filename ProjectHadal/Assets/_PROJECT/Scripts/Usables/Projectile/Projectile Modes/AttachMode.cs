using UnityEngine;

// C: Jin
[System.Serializable]
public class AttachMode : ProjectileMode
{
    public delegate void AttachEvent();
    public event AttachEvent SwitchedToAttachEvent;

    public override void Setup(Rigidbody rb, Transform rTransform)
    {
        base.Setup(rb, rTransform);
        mode = ProjectileModeEnum.ATTACH;
    }

    public override void FirstFrameSetup()
    {
        frameSetupCompleted = true;
        SwitchedToAttachEvent?.Invoke();
    }

    public override void DoUpdate()
    {
        if (!frameSetupCompleted) FirstFrameSetup();
    }
}
