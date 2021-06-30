using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Networking;
using Kit;

namespace Hadal.AI
{
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance;
        public AIBrain brain;

        NetworkEventManager neManager;

        [Header("Spawn Settings")]
        [SerializeField] private bool enableAIRandomSpawn = false;
        [SerializeField] private List<Transform> spawnPositions;

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        IEnumerator Start()
        {
			if (brain == null)
				brain = FindObjectOfType<AIBrain>();
			
            neManager = NetworkEventManager.Instance;

            if (enableAIRandomSpawn)
            {
                FindObjectOfType<AITransformHandler>().MoveAndRotate(spawnPositions[(int)Random.Range(0, spawnPositions.Count)]);
            }

            //! The null is to make sure the AI does not go to the NavPoint of where its spawned.
            yield return new WaitForSeconds(0.1f);

            brain.NavigationHandler.SkipCurrentPoint(true);

        }

    }
}
