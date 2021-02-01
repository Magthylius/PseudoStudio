using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.IO;
using UnityEngine;

//Created by Jet
namespace Hadal.Controls
{
    public class PlayerManager : MonoBehaviour
    {
        private const string PrefabFolder = "PhotonControlPrefabs";
        private const string PrefabName = "Player";
        private PhotonView _pView;
        private GameObject _controller;

        private void Awake() => _pView = GetComponent<PhotonView>();
        private void Start()
        {
            if (_pView.IsMine) CreateController();
        }

        public void Die()
        {
            PhotonNetwork.Destroy(_controller);
            CreateController();
            var c = _controller.GetComponent<PlayerController>();
            if (c == null) return;
            c.ResetController();
        }

        private void CreateController()
        {
            Transform spawnTrans = SpawnManager.instance.GetSpawnPoint();
            _controller = PhotonNetwork.Instantiate(GetPrefabPath(), spawnTrans.position, spawnTrans.rotation, 0, DefaultObjectArray());
        }
        private string GetPrefabPath() => Path.Combine(PrefabFolder, PrefabName);
        private object[] DefaultObjectArray() => new object[] { _pView.ViewID };
    }
}