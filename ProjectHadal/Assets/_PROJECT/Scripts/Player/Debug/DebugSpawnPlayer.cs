using UnityEngine;
using NaughtyAttributes;
using PlayerM = Hadal.Player.PlayerManager;
using Photon.Pun;

namespace Hadal.Player
{
    public class DebugSpawnPlayer : MonoBehaviour
    {
        [SerializeField, ReadOnly] GameObject _controller;
        [Foldout("Settings"), SerializeField] bool isOfflineMode = true;
        [Foldout("Settings"), SerializeField] Transform spawnHereTransform;

        private void Awake()
        {
            PhotonNetwork.OfflineMode = isOfflineMode;
            var prefab = Resources.Load(PathManager.PlayerManagerPrefabPath);
            if(prefab is null) return;
            _controller = (GameObject)Instantiate(prefab, spawnHereTransform.position, spawnHereTransform.rotation);
            _controller.GetComponent<PlayerM>().IsOnNetwork = !isOfflineMode;
        }
    }
}