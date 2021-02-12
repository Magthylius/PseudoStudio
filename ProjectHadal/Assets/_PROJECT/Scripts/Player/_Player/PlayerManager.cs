using Hadal.Legacy;
using Photon.Pun;
using System.IO;
using UnityEngine;

//Created by Jet
namespace Hadal.Player
{
    public class PlayerManager : MonoBehaviour
    {
        public bool IsOnNetwork { get; set; } = true;
        private const string PrefabFolder = "Prefabs/Player";
        private const string PrefabName = "Player";
        private PhotonView _pView;
        private GameObject _controller;

        private void Awake() => _pView = GetComponent<PhotonView>();
        private void Start()
        {
            if(IsOnNetwork)
            {
                if (_pView.IsMine) CreateNetworkController();
                return;
            }
            CreateController();
        }

        public void TryToDie()
        {
            if(IsOnNetwork)
            {
                NetworkDie();
                return;
            }
            Die();
        }

        #region Network Methods

        private void NetworkDie()
        {
            if (!IsOnNetwork) return;
            PhotonNetwork.Destroy(_controller);
            CreateNetworkController();
            var c = _controller.GetComponent<PlayerController>();
            if (c == null) return;
            c.ResetController();
        }
        private void CreateNetworkController()
        {
            if (!IsOnNetwork) return;
            Transform spawnTrans = SpawnManager.instance.GetSpawnPoint();
            _controller = PhotonNetwork.Instantiate(GetPrefabPath(), spawnTrans.position, spawnTrans.rotation, 0, DefaultObjectArray());
            _controller.GetComponent<PlayerController>().InjectManager(this);
        }

        #endregion

        #region Local Methods

        private void Die()
        {
            if (IsOnNetwork) return;
            Destroy(_controller);
            CreateController();
            var c = _controller.GetComponent<PlayerController>();
            if (c is null) return;
            c.ResetController();
        }
        private void CreateController()
        {
            if (IsOnNetwork) return;
            var prefab = Resources.Load(GetPrefabPath());
            if (prefab is null) return;
            _controller = (GameObject)Instantiate(prefab, transform.position, transform.rotation);
            _controller.GetComponent<PlayerController>().InjectManager(this);
        }

        #endregion

        private string GetPrefabPath() => Path.Combine(PrefabFolder, PrefabName);
        private object[] DefaultObjectArray() => new object[] { _pView.ViewID };
    }
}