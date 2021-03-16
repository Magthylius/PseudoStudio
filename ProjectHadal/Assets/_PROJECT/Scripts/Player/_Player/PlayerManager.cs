using Hadal.Legacy;
using Hadal.Networking;
using Photon.Pun;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//! Created by Jet, E: Jon
namespace Hadal.Player
{
    public class PlayerManager : MonoBehaviour
    {
        public bool IsOnNetwork { get; set; } = !NetworkEventManager.Instance.isOfflineMode;
        private const string PrefabFolder = "Prefabs/Player";
        private const string PrefabName = "Player";
        private PhotonView _pView;
        //private GameObject player;

        List<PlayerController> playerList;
        NetworkEventManager neManager;

        private void Awake() => _pView = GetComponent<PhotonView>();
        private void OnEnable()
        {
            neManager = NetworkEventManager.Instance;

            playerList = new List<PlayerController>();

            if (IsOnNetwork)
            {
                if (_pView.IsMine)
                {
                    foreach (KeyValuePair<int, Photon.Realtime.Player> playerDict in neManager.AllPlayers)
                    {
                        SpawnPlayer(playerDict.Value);
                    }

                    NetworkEventManager.Instance.PlayerEnteredEvent += SpawnPlayer;
                    NetworkEventManager.Instance.PlayerLeftEvent += TryToKill;
                }
                return;
            }

            SpawnPlayer(neManager.LocalPlayer);
        }

        void OnDisable()
        {
            if (IsOnNetwork)
            {
                if (_pView.IsMine)
                {
                    //NetworkEventManager.Instance.LeftRoomEvent -= TryToKill;
                    NetworkEventManager.Instance.PlayerEnteredEvent -= SpawnPlayer;
                    NetworkEventManager.Instance.PlayerLeftEvent -= TryToKill;
                }
                return;
            }
        }

        public void TryToKill(Photon.Realtime.Player targetPlayer)
        {
            foreach (PlayerController controller in playerList)
            {
                if (controller.AttachedPlayer == targetPlayer)
                {
                    print("Try death");
                    if (IsOnNetwork)
                    {
                        print("Networked death");
                        if (controller.gameObject != null) NetworkKill(controller.gameObject);
                        return;
                    }
                    if (controller.gameObject != null) Kill(controller.gameObject);
                }
            }

            Debug.LogWarning("Tried to kill a non-player!");
            return;
        }

        #region Player
        void SpawnPlayer(Photon.Realtime.Player player)
        {
            //if (!player.IsMasterClient) return;
            if (IsOnNetwork) CreateNetworkController(player);
            else CreateLocalController(player);
        }
        #endregion

        #region Network Methods

        private void NetworkKill(GameObject player)
        {
            if (!IsOnNetwork || gameObject == null) return;
            playerList.Remove(GetController(player));
            neManager.RemovePlayer(player);
            PhotonNetwork.Destroy(player);
        }
        private void CreateNetworkController(Photon.Realtime.Player photonPlayer)
        {
            if (!IsOnNetwork) return;
            Transform spawnTrans = SpawnManager.instance.GetSpawnPoint();
            GameObject player = PhotonNetwork.Instantiate(GetPrefabPath(), spawnTrans.position, spawnTrans.rotation, 0, DefaultObjectArray());
            PlayerController controller = player.GetComponent<PlayerController>();

            playerList.Add(controller);
            controller.InjectDependencies(this, photonPlayer);

            if (photonPlayer != neManager.LocalPlayer)
            {
                controller.TransferOwnership(photonPlayer);
                controller.HandlePhotonView(false);
            }
            else
            {
                controller.HandlePhotonView(true);
            }

            /*if (playerList.Count > 0)
            {
                foreach (PlayerController pControl in playerList)
                {
                    //print(pControl.GetInfo.PhotonInfo.PView.ViewID + ", " + GetController(neManager.LocalPlayer).ViewID);
                    //print(pControl.GetInfo.PhotonInfo.PView.ViewID == GetController(neManager.LocalPlayer).ViewID);
                    pControl.HandlePhotonView(pControl.GetInfo.PhotonInfo.PView.ViewID == GetController(neManager.LocalPlayer).ViewID);
                }
            }*/


            neManager.AddPlayer(player);
        }

        #endregion

        #region Local Methods

        private void Kill(GameObject player)
        {
            if (IsOnNetwork || gameObject == null) return;
            neManager.RemovePlayer(player);
            Destroy(player);
        }
        private void CreateLocalController(Photon.Realtime.Player photonPlayer)
        {
            if (IsOnNetwork) return;
            var prefab = Resources.Load(GetPrefabPath());
            if (prefab is null) return;
            GameObject player = (GameObject)Instantiate(prefab, transform.position, transform.rotation);
            PlayerController controller = player.GetComponent<PlayerController>();

            playerList.Add(controller);
            controller.InjectDependencies(this, photonPlayer);

            neManager.AddPlayer(player);
        }

        #endregion

        private string GetPrefabPath() => Path.Combine(PrefabFolder, PrefabName);
        private object[] DefaultObjectArray() => new object[] { _pView.ViewID };
        PlayerController GetController(Photon.Realtime.Player player)
        {
            foreach (PlayerController controller in playerList) if (controller.AttachedPlayer == player) return controller;
            return null;
        }
        PlayerController GetController(GameObject playerObject) => playerObject.GetComponent<PlayerController>();
    }
}