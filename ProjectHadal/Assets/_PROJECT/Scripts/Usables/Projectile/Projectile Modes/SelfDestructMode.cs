[System.Serializable]
public class SelfDestructMode : ProjectileMode
{
    public float range;
    public float outwardForce;
    public float inwardForce;

    public bool destroyObject;

    public override void Setup()
    {
        mode = ProjectileModeEnum.SELF_DESTRUCT;
    }
}
