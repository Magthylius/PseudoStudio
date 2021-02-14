using UnityEngine;

[System.Serializable]
public class PropulsionMode : ProjectileMode
{
    [Header("Propulsion settings")]
    public float force;
    public float linearDrag;

    public override void Setup(Rigidbody rb)
    {
        rigidbody = rb;
        mode = ProjectileModeEnum.PROPULSION;
    }

    public override void FirstFrameSetup()
    {
        frameSetupCompleted = false;
        rigidbody.drag = linearDrag;
    }

    public override void DoUpdate()
    {
        if (!frameSetupCompleted) FirstFrameSetup();

        rigidbody.AddForce(Vector3.forward * force, ForceMode.Force);
    }
}
