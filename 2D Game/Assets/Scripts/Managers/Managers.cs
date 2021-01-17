using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Managers
{

    [RequireComponent(typeof(PlayerManager))]
    public class Managers : MonoBehaviour
    {
        public static Managers Instance { get; private set; }
        public static PlayerManager Player { get; private set; }

        public bool AreReady { get; private set; } = false;

        private List<IManager> startSequence;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (AreReady)
            {
                return;
            }

            Player = GetComponent<PlayerManager>();

            startSequence = new List<IManager>
            {
                Player
            };

            StartCoroutine(StartManagers());

        }

        private IEnumerator StartManagers()
        {
            foreach (var manager in startSequence)
            {
                try
                {
                    manager.Startup();
                }
                catch (NullReferenceException)
                {
                    Debug.LogWarning("NullReferenceException: One of the managers is missing!");
                }
            }

            yield return null;

            int numModules = startSequence.Count;
            int numReady = 0;

            while (numReady < numModules)
            {
                int lastReady = numReady;
                numReady = 0;

                foreach (IManager manager in startSequence)
                {
                    if (manager.Status == ManagerStatus.Started)
                    {
                        numReady++;
                    }
                }

                if (numReady > lastReady)
                    Debug.Log("Progress: " + numReady + "/" + numModules);

                yield return null;
            }

            Debug.Log("All managers started up");
            AreReady = true;
        }
    }
}
