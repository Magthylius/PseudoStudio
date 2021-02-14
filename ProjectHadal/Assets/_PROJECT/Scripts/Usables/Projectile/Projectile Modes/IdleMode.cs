[System.Serializable]
public class IdleMode : ProjectileMode
{
    public bool destroyObject;

    public override void Setup()
    {
        mode = ProjectileModeEnum.IDLE;
    }
}
