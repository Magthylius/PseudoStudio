using Hadal.Legacy;
using Hadal.Networking;
using Photon.Pun;
using System.IO;
using UnityEngine;

//! Created by Jet, E: Jon
namespace Hadal.Player
{
    public class PlayerManager : MonoBehaviour
    {
        public bool IsOnNetwork { get; set; } = true;
        private const string PrefabFolder = "Prefabs/Player";
        private const string PrefabName = "Player";
        private PhotonView _pView;
        private GameObject player;

        private void Awake() => _pView = GetComponent<PhotonView>();
        private void OnEnable()
        {
            if (IsOnNetwork)
            {
                if (_pView.IsMine)
                {
                    CreateNetworkController();
                    NetworkEventManager.Instance.LeftRoomEvent += TryToDie;
                }
                return;
            }
            CreateController();
        }

        void OnDisable()
        {
            if (IsOnNetwork)
            {
                if (_pView.IsMine)
                {
                    NetworkEventManager.Instance.LeftRoomEvent -= TryToDie;
                }
                return;
            }
        }

        public void TryToDie()
        {
            print("Try death");
            if (IsOnNetwork)
            {
                print("Networked death");
                NetworkDie();
                return;
            }
            Die();
        }

        #region Network Methods

        private void NetworkDie()
        {
            if (!IsOnNetwork) return;
            PhotonNetwork.Destroy(player);
            /*CreateNetworkController();
            var c = player.GetComponent<PlayerController>();
            if (c == null) return;
            c.ResetController();*/
        }
        private void CreateNetworkController()
        {
            if (!IsOnNetwork) return;
            Transform spawnTrans = SpawnManager.instance.GetSpawnPoint();
            player = PhotonNetwork.Instantiate(GetPrefabPath(), spawnTrans.position, spawnTrans.rotation, 0, DefaultObjectArray());
            player.GetComponent<PlayerController>().InjectManager(this);
        }

        #endregion

        #region Local Methods

        private void Die()
        {
            if (IsOnNetwork) return;
            Destroy(player);
            /*CreateController();
            var c = player.GetComponent<PlayerController>();
            if (c is null) return;
            c.ResetController();*/
        }
        private void CreateController()
        {
            if (IsOnNetwork) return;
            var prefab = Resources.Load(GetPrefabPath());
            if (prefab is null) return;
            player = (GameObject)Instantiate(prefab, transform.position, transform.rotation);
            player.GetComponent<PlayerController>().InjectManager(this);
        }

        #endregion

        private string GetPrefabPath() => Path.Combine(PrefabFolder, PrefabName);
        private object[] DefaultObjectArray() => new object[] { _pView.ViewID };
    }
}