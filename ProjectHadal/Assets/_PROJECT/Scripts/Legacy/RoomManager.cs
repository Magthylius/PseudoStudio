using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

//! E: Jon
namespace Hadal.Legacy
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        public static RoomManager Instance;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.buildIndex == 0) return;
            PhotonNetwork.Instantiate(PathManager.PlayerManagerPrefabPath, Vector3.zero, Quaternion.identity);
            //CreateAI();
        }

        void CreateAI()
        {
            if(PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Instantiate(Path.Combine("Prefabs/Animol", "Animal"),
                                                transform.position,
                                                transform.rotation,
                                                0);
            }
        
        }
    }
}