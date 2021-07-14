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
        public GameObject aiPrefab;

        NetworkEventManager neManager;

        [Header("Spawn Settings")]
        [SerializeField] private bool enableAIRandomSpawn = false;
        [SerializeField] private List<Transform> spawnPositions;

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;

            if (enableAIRandomSpawn)
            {
                Transform spawnPoints = spawnPositions[(int)Random.Range(0, spawnPositions.Count)];
                Instantiate(aiPrefab, spawnPoints.position, spawnPoints.rotation);
            }


        }

        // IEnumerator Start()
        // {
        //     if (brain == null)
        //         brain = FindObjectOfType<AIBrain>();

        //     neManager = NetworkEventManager.Instance;

        //     if (enableAIRandomSpawn)
        //     {


        //     }

        //     //! The null is to make sure the AI does not go to the NavPoint of where its spawned.
        //     yield return new WaitForSeconds(0.1f);

        //     brain.NavigationHandler.SkipCurrentPoint(true);

        // }

    }
}
