using System;
using UnityEngine;
using NaughtyAttributes;
using PlayerM = Hadal.Controls.PlayerManager;
using Photon.Pun;

namespace Hadal.Debugging
{
    public class DebugSpawnPlayer : MonoBehaviour
    {
        [SerializeField, ReadOnly] GameObject _controller;
        [Foldout("Settings"), SerializeField] bool isOfflineMode = true;
        [Foldout("Settings"), SerializeField] Vector3 spawnPosition;
        [Foldout("Settings"), SerializeField] Quaternion spawnRotation;

        private void Awake()
        {
            PhotonNetwork.OfflineMode = isOfflineMode;
            var prefab = Resources.Load(PathManager.PlayerManagerPrefabPath);
            if(prefab is null) return;
            _controller = (GameObject)Instantiate(prefab, spawnPosition, spawnRotation);
            _controller.GetComponent<PlayerM>().IsOnNetwork = !isOfflineMode;
        }
    }
}