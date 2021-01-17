using Game.Characters;
using System.Collections;

namespace Game.Managers
{
    public abstract class CharacterManager : BaseCharacter, IManager
    {
        public abstract ManagerStatus Status { get; protected set; }

        public abstract IEnumerator LoadAndCache();

        public abstract IEnumerator Load();

        public abstract void Startup();
    }
}
