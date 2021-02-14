[System.Serializable]
public class ImpulseMode : ProjectileMode
{
    public float force;
    public float linearDrag;

    public override void Setup()
    {
        mode = ProjectileModeEnum.IMPULSE;
    }
}
