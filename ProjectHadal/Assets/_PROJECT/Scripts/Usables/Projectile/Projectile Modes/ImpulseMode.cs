using UnityEngine;
using Hadal;
using Hadal.Usables.Projectiles;
// C: Jon E:Jin
[System.Serializable]
public class ImpulseMode : ProjectileMode
{
    [Header("Impulse settings")]
    public Vector3 direction;
    public float force;
    public float linearDrag;
    public delegate void ModeSwapEvent();
    public event ModeSwapEvent ModeSwapped;

    public override void Setup(Rigidbody rb, Transform rTransform)
    {
        base.Setup(rb, rTransform);
        mode = ProjectileModeEnum.IMPULSE;
    }

    public override void FirstFrameSetup()
    {
        frameSetupCompleted = false;
        rigidbody.drag = linearDrag;

        rigidbody.AddRelativeForce(direction.normalized * force, ForceMode.Impulse);
    }

    public override void DoUpdate()
    {
        if (!frameSetupCompleted) FirstFrameSetup();
    }

    public void OverrideForce(float overridingForce) => force = overridingForce;
    public void OverrideForce(float overridingForce, bool isModeSwap)
    {
        if(isModeSwap)
        {
            Debug.LogError(ModeSwapped.GetInvocationList().Length);  
            ModeSwapped?.Invoke();
            Debug.LogError("Event called");
        }     
        force = overridingForce;
    }
}
