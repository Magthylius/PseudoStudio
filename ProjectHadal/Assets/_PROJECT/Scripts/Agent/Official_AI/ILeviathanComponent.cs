namespace Hadal.AI
{
    public interface ILeviathanComponent
    {
        //! Only for PreUpdate & MainUpdate only
        UpdateMode LeviathanUpdateMode { get; }
        void Initialise(AIBrain brain);
        void DoUpdate(in float deltaTime);
        void DoFixedUpdate(in float fixedDeltaTime);
        void DoLateUpdate(in float deltaTime);
    }

    public enum UpdateMode
    {
        PreUpdate,
        MainUpdate,
        FixedUpdate,
        PostUpdate,
        LateUpdate
    }
}
