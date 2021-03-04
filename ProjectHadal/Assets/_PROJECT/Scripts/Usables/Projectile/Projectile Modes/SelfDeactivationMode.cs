using UnityEngine;

// C: Jon
[System.Serializable]
public class SelfDeactivationMode : ProjectileMode
{
    [Header("Deactivation settings")]
    public bool destroyObject;
    public delegate void SelfDeactivateEvent(bool isSwap);
    public event SelfDeactivateEvent selfDeactivated;

    public override void Setup(Rigidbody rb, Transform rTransform)
    {
        base.Setup(rb, rTransform);
        mode = ProjectileModeEnum.SELF_DEACTIVATE;
    }

    public override void FirstFrameSetup()
    {
        frameSetupCompleted = true;
        selfDeactivated?.Invoke(false);
    }

    public override void DoUpdate()
    {
        if (!frameSetupCompleted) FirstFrameSetup();
    }
}
