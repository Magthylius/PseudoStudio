using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Networking;
using Kit;
using UnityEngine.SceneManagement;

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

            SpawnInCorrectScene();
        }

        void SpawnInCorrectScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            string sceneName = currentScene.name;
            if (sceneName == "Post Vertical Slice")
            {
                if (enableAIRandomSpawn)
                {
                    Transform spawnPoints = spawnPositions[(int)Random.Range(0, spawnPositions.Count)];
                    Instantiate(aiPrefab, spawnPoints.position, spawnPoints.rotation);
                }
            }
        }

    }
}
