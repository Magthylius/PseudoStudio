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

        NetworkEventManager neManager;

        [Header("Spawn Settings")] 
        [SerializeField] private bool enableAIRandomSpawn = false;
        [SerializeField] private List<Transform> spawnPositions;

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        void Start()
        {
            neManager = NetworkEventManager.Instance;

            if (enableAIRandomSpawn)
            {
                FindObjectOfType<AITransformHandler>().Move(spawnPositions[(int)Random.Range(0, spawnPositions.Count)].position);
            }
        }

    }
}
