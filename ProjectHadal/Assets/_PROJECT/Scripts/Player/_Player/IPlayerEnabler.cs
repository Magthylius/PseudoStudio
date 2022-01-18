namespace Hadal.Player
{
    internal interface IPlayerEnabler
    {
        bool AllowUpdate { get; }
        void Enable();
        void Disable();
        void ToggleEnablility();
    }
}