//Created by Jet
namespace Hadal
{
    public interface IUnityServicer
    {
        float ElapsedTime { get; }
        float DeltaTime { get; }
        void DoUpdate(in float deltaTime);
    }
}