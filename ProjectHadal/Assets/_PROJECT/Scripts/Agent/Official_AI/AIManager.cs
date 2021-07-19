using System.Collections;
using System.Collections.Generic;
using Hadal.AI.Information;
using Hadal.Networking;
using Hadal.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

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
                GameObject ai = Instantiate(prefab, spawnPoints.position, spawnPoints.rotation);
                
               // LocalPlayerData.PlayerController.InjectAIDependencies(ai.GetComponent<AIPackageInfo>().Brain.transform);
            }
        }

        void NetworkSpawnInCorrectScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            string sceneName = currentScene.name;
            if (sceneName == NetworkEventManager.Instance.InGameScene)
            {
                Debug.LogWarning("Network spawn ai!");
                
                if (PhotonNetwork.IsMasterClient)
                {
                    Transform spawnPoints = spawnPositions[(int)Random.Range(0, spawnPositions.Count)];
                    GameObject ai = PhotonNetwork.Instantiate(AIPackagePrefabPath, spawnPoints.position, spawnPoints.rotation);
                    //StartCoroutine(InitAINetworked(ai));
                }
                else
                {
                    //! Wait for AI to spawn
                    //StartCoroutine(InitAI());
                    
                    /*IEnumerator InitAI()
                    {
                        AIBrain brain = FindObjectOfType<AIBrain>();

                        while (brain == null)
                        {
                            brain = FindObjectOfType<AIBrain>();
                            Debug.LogWarning("waiting for AIbrain");
                            yield return null;
                        }
                        
                        while (LocalPlayerData.PlayerController == null)
                        {
                            Debug.LogWarning("waiting for playercontroller");
                            yield return null;
                        }
                        
                        Debug.LogWarning("AI UI init!");
                        LocalPlayerData.PlayerController.InjectAIDependencies(brain.transform);
                    }*/
                }
            }
        }

        
        /*IEnumerator InitAINetworked(GameObject ai)
        {
            do 
            {
                Debug.LogWarning($"waiting for playercontroller: {LocalPlayerData.PlayerController != null}");
                yield return null;
            } while (true);
                        
            Debug.LogWarning("AI UI init!");
            LocalPlayerData.PlayerController.InjectAIDependencies(ai.GetComponent<AIPackageInfo>().Brain.transform);
        }*/
    }
}