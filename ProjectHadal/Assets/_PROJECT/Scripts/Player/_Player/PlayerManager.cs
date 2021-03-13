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
        public bool IsOnNetwork { get; set; } = true;
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

            if (IsOnNetwork)
            {
                if (_pView.IsMine)
                {
                    playerList = new List<PlayerController>();

                    foreach (KeyValuePair<int, Photon.Realtime.Player> playerDict in neManager.AllPlayers)
                    {
                        SpawnPlayer(playerDict.Value);
                    }

                    NetworkEventManager.Instance.PlayerEnteredEvent += SpawnPlayer;
                    NetworkEventManager.Instance.PlayerLeftEvent += TryToKill;
                }
                return;
            }
            //CreateController();
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
            if (IsOnNetwork) CreateNetworkController(player);
            else CreateLocalController(player);
        }
        #endregion

        #region Network Methods

        private void NetworkKill(GameObject player)
        {
            if (!IsOnNetwork || gameObject == null) return;
            playerList.Remove(GetController(player));
            PhotonNetwork.Destroy(player);
            /*CreateNetworkController();
            var c = player.GetComponent<PlayerController>();
            if (c == null) return;
            c.ResetController();*/
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
            }

            if (playerList.Count > 1)
            {
                foreach (PlayerController pControl in playerList)
                {
                    //print(pControl.GetInfo.PhotonInfo.PView.ViewID + ", " + GetController(neManager.LocalPlayer).ViewID);
                    //print(pControl.GetInfo.PhotonInfo.PView.ViewID == GetController(neManager.LocalPlayer).ViewID);
                    pControl.HandlePhotonView(pControl.GetInfo.PhotonInfo.PView.ViewID == GetController(neManager.LocalPlayer).ViewID);
                }
            }
        }

        #endregion

        #region Local Methods

        private void Kill(GameObject player)
        {
            if (IsOnNetwork || gameObject == null) return;
            Destroy(player);
            /*CreateController();
            var c = player.GetComponent<PlayerController>();
            if (c is null) return;
            c.ResetController();*/
        }
        private void CreateLocalController(Photon.Realtime.Player photonPlayer)
        {
            if (IsOnNetwork) return;
            var prefab = Resources.Load(GetPrefabPath());
            if (prefab is null) return;
            GameObject player = (GameObject)Instantiate(prefab, transform.position, transform.rotation);
            player.GetComponent<PlayerController>().InjectDependencies(this, photonPlayer);
            playerList.Add(player.GetComponent<PlayerController>());
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