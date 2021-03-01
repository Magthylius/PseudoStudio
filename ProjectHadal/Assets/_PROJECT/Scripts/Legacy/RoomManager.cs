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
            //!Singleton
            /*if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            //DontDestroyOnLoad(gameObject);
            Instance = this;*/

            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        //!OnEnable and OnDisable is the central to photon functioning correctly
        //!these two methods are the only base methods that needs to be called in order to override
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