using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Networking;
using Kit;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace Hadal.AI
{
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance;

        [Header("Spawn Settings")]
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
                if (PhotonNetwork.IsMasterClient)
                {
                    Transform spawnPoints = spawnPositions[(int)Random.Range(0, spawnPositions.Count)];
                    PhotonNetwork.Instantiate("AI Data/_OfficialAI/@AI Package", spawnPoints.position, spawnPoints.rotation);
                }
            }
        }

    }
}
