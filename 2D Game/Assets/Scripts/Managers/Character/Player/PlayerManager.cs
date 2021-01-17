using System.Collections;
using UnityEngine;

namespace Game.Managers
{
    public class PlayerManager : CharacterManager
    {
        public override ManagerStatus Status { get; protected set; }

        private Managers managers;

        public override void Startup()
        {
            Debug.Log("Player Manager is starting...");

            /// TODO: Load stats from save files

            Status = ManagerStatus.Initializing;

            /// TODO: Add long-runing startups tasks set Status to Initializing 

            StartCoroutine(LoadAndCache());

            Debug.Log(Status);
        }

        void Update()
        {
            if (!managers.AreReady)
            {
                Debug.LogWarning("Managers not ready yet.");
                return;
            }
        }

        public override IEnumerator LoadAndCache()
        {
            yield return StartCoroutine(Load());

            Status = ManagerStatus.Started;
            Debug.Log(Status);
        }

        public override IEnumerator Load()
        {
            managers = GetComponent<Managers>();

            Initialize();

            yield return null;
        }
    }
}
