using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Networking;

namespace Hadal.AI
{
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance;

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
            patrolPositions = patrolPositionParent.GetComponentsInChildren<Transform>();
            NetworkEventManager.Instance.SpawnAI(spawnPosition.position, spawnPosition.rotation);
        }

        public Transform[] GetPositions() => patrolPositions;
    }
}
