using UnityEngine;

// C: Jon
[System.Serializable]
public class ImpulseMode : ProjectileMode
{
    [Header("Impulse settings")]
    public Vector3 direction;
    public float force;
    public float linearDrag;

    public override void Setup(Rigidbody rb)
    {
        rigidbody = rb;
        mode = ProjectileModeEnum.IMPULSE;
    }

    public override void FirstFrameSetup()
    {
        frameSetupCompleted = false;
        rigidbody.drag = linearDrag;

        rigidbody.AddForce(direction * force, ForceMode.Impulse);
    }

    public override void DoUpdate()
    {
        if (!frameSetupCompleted) FirstFrameSetup();
    }
}
