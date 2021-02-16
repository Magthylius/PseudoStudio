using UnityEngine;

// C: Jon
[System.Serializable]
public class PropulsionMode : ProjectileMode
{
    [Header("Propulsion settings")]
    public float force;
    public float linearDrag;

    public override void Setup(Rigidbody rb, Transform rTransform)
    {
        rootTransform = rTransform;
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

        rigidbody.AddForce(rootTransform.forward * force, ForceMode.Force);
        //rigidbody.AddRelativeForce(transform.forward * force, ForceMode.Force);
    }
}
