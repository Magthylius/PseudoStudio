using UnityEngine;

// C: Jon
[System.Serializable]
public class SelfDeactivationMode : ProjectileMode
{
    [Header("Deactivation settings")]
    public float attachTimer;
    public bool destroyObject;
    public delegate void SelfDeactivateEvent();
    public event SelfDeactivateEvent selfDeactivated;

    public override void Setup(Rigidbody rb, Transform rTransform)
    {
        base.Setup(rb, rTransform);
        mode = ProjectileModeEnum.SELF_DEACTIVATE;
    }

    public override void FirstFrameSetup()
    {
        Debug.Log("First FrameEvent called");
        frameSetupCompleted = true;
        selfDeactivated?.Invoke();
    }

    public override void DoUpdate()
    {
        if (!frameSetupCompleted) FirstFrameSetup();
    }
}
