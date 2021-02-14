[System.Serializable]
public class SelfDeactivationMode : ProjectileMode
{
    public bool destroyObject;

    public override void Setup()
    {
        mode = ProjectileModeEnum.SELF_DEACTIVATE;
    }
}
