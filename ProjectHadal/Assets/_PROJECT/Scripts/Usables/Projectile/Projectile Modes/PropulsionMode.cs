[System.Serializable]
public class PropulsionMode : ProjectileMode
{
    public float force;
    public float linearDrag;

    public override void Setup()
    {
        mode = ProjectileModeEnum.PROPULSION;
    }
}
