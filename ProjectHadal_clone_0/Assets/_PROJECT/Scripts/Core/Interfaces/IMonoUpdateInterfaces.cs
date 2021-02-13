using System.Collections.Generic;

namespace Hadal
{
    public interface IMonoUpdatable
    {
        void DoUpdate(in float deltaTime);
    }

    public interface IMonoUpdater
    {
        List<IMonoUpdatable> Updatables {get;}
        void UpdateAll(in float deltaTime);
    }
}