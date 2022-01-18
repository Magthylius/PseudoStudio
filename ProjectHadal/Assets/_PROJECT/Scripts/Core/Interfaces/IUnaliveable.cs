//Created by Jet
namespace Hadal
{
    public interface IUnalivable
    {
        bool IsUnalive { get; }
        bool IsDown { get; }
        int GetCurrentHealth { get; }
        int GetMaxHealth { get; }
        float GetHealthRatio { get; }

        void ResetHealth();
        void CheckHealthStatus();
    }
}