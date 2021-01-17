using System.Collections;

namespace Game.Managers
{
    public interface IManager
    {
        ManagerStatus Status { get; }

        void Startup();

        IEnumerator LoadAndCache();

        IEnumerator Load();
    }
}
