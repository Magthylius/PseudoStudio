using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Networking;

namespace Hadal.AI
{
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance;

        NetworkEventManager neManager;

        [Header("Settings")]
        [SerializeField] bool enableAIInit = true;

        [Header("References")]
        public Transform patrolPositionParent;
        public Transform spawnPosition;

        Transform[] patrolPositions;

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        void Start()
        {
            neManager = NetworkEventManager.Instance;

            if (enableAIInit)
            {
                patrolPositions = patrolPositionParent.GetComponentsInChildren<Transform>();
                if (neManager.IsMasterClient) neManager.SpawnAIEssentials(spawnPosition.position, spawnPosition.rotation);
            }
        }

        public Transform[] GetPositions() => patrolPositions;
    }
}
