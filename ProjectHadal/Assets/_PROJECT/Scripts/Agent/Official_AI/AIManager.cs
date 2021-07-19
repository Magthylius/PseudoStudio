
using System.Collections.Generic;
using Hadal.Networking;
using UnityEngine;
using Photon.Pun;

namespace Hadal.AI
{
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance;

        [Header("Spawn Settings")]
        [SerializeField] private List<Transform> spawnPositions;

        private const string AIPackagePrefabPath = "AI Data/_OfficialAI/@AI Package";

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;

            if (PhotonNetwork.IsConnected && !PhotonNetwork.OfflineMode)
            {
                NetworkSpawnInCorrectScene();
            }
            else
            {
                LocalSpawnInCorrectScene();
            }
        }

        private string targetSceneName = "Post Vertical Slice";

        void LocalSpawnInCorrectScene()
        {
            
            if (NetworkEventManager.Instance.IsInGame)
            {
                Transform spawnPoints = spawnPositions[(int)Random.Range(0, spawnPositions.Count)];
                GameObject prefab = (GameObject)Resources.Load(AIPackagePrefabPath);
                Instantiate(prefab, spawnPoints.position, spawnPoints.rotation);
            }
        }

        void NetworkSpawnInCorrectScene()
        {
            
            if (NetworkEventManager.Instance.IsInGame)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    Transform spawnPoints = spawnPositions[(int)Random.Range(0, spawnPositions.Count)];
                    PhotonNetwork.Instantiate(AIPackagePrefabPath, spawnPoints.position, spawnPoints.rotation);
                }
            }
        }
        
    }
}