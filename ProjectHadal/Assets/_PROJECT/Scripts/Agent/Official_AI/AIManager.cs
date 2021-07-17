using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Hadal.Player;

namespace Hadal.AI
{
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance;

        [Header("Spawn Settings")]
        [SerializeField] private List<Transform> spawnPositions;
        [SerializeField] string nameOfScene;

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
            Scene currentScene = SceneManager.GetActiveScene();
            string sceneName = currentScene.name;
            if (sceneName == targetSceneName)
            {
                Transform spawnPoints = spawnPositions[(int)Random.Range(0, spawnPositions.Count)];
                GameObject prefab = (GameObject)Resources.Load(AIPackagePrefabPath);
                Instantiate(prefab, spawnPoints.position, spawnPoints.rotation);
            }
        }

        void NetworkSpawnInCorrectScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            string sceneName = currentScene.name;
            if (sceneName == targetSceneName)
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
