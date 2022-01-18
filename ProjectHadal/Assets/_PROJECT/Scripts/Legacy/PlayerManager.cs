using UnityEngine;
using Photon.Pun;
using System.IO;

namespace Hadal.Legacy
{
    public class PlayerManager : MonoBehaviour
    {
        PhotonView PV;
        GameObject controller;

        private void Awake()
        {
            PV = GetComponent<PhotonView>();
        }

        void Start()
        {
            if (PV.IsMine)
            {
                CreateController();
            }
        }

        void Update()
        {

        }

        /// <summary>
        /// Creates a player controller photonprefab
        /// </summary>
        void CreateController()
        {
            Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
            controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnPoint.position, spawnPoint.rotation, 0, new object[] { PV.ViewID });
        }

        public void Die()
        {
            PhotonNetwork.Destroy(controller);
            CreateController();
        }
    }
}