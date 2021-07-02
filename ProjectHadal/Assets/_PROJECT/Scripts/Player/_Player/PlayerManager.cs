using ExitGames.Client.Photon;
using Hadal.Legacy;
using Hadal.Networking;
using Photon.Pun;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

//! Created by Jet, E: Jon
namespace Hadal.Player
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;
        
        public delegate void AddAPlayerEvent();
        public static event AddAPlayerEvent AddPlayerEvent;
        public event Action OnAllPlayersReadyEvent;

        public bool isOnNetwork = false;
        private const string PrefabFolder = "Prefabs/Player";
        private const string PrefabName = "Player";
        private PhotonView _pView;
        public bool allPlayerReady;
        [SerializeField] List<PlayerController> playerList;
        NetworkEventManager neManager;

        [Header("Offline Player Dummies")]
        private int DummyPlayerCount;
        private bool DummyMirrorsMovement;

        private PlayerController localPlayerController;
        public PlayerController LocalPlayerController => localPlayerController;

        private void Awake()
        {
            _pView = GetComponent<PhotonView>();
            
            if (Instance != null) Destroy(this);
            else Instance = this;
        }
        private void OnEnable()
        {
            neManager = NetworkEventManager.Instance;

            playerList = new List<PlayerController>();
            SetDummySettings(neManager.DummyCount, neManager.DummyMirrorsMovement);

            if (IsOnNetwork)
            {
                if (_pView.IsMine)
                {
                    foreach (KeyValuePair<int, Photon.Realtime.Player> playerDict in neManager.AllPlayers)
                    {
                        SpawnPlayer(playerDict.Value);
                        print("Player Spawned");
                    }

                    NetworkEventManager.Instance.PlayerEnteredEvent += SpawnPlayer;
                    NetworkEventManager.Instance.PlayerLeftEvent += TryToKill;
                    NetworkEventManager.Instance.AddListener(ByteEvents.PLAYER_SPAWNED, PlayerReadyEvent);
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
                    NetworkEventManager.Instance.RemoveListener(ByteEvents.PLAYER_SPAWNED, PlayerReadyEvent);
                }
                return;
            }
        }

        private void Update()
        {
            if (!_pView.IsMine)
                return;

            if (allPlayerReady)
                return;

            for(int i = 0; i < playerList.Count; i++)
            {
                if(playerList[i].GetPlayerReady())
                {
                    allPlayerReady = true;
                }
                else
                {
                    allPlayerReady = false;
                    return;
                }
            }

            if (allPlayerReady)
            {
                NetworkEventManager.Instance.RaiseEvent(ByteEvents.GAME_ACTUAL_START, null, SendOptions.SendReliable);
                
                //! Host start games here
                /*LoadingManager.Instance.StartEndLoad();
                InstantiatePViewList();
                localPlayerController.Mover.ToggleEnablility(true);
                localPlayerController.TrackNamesOnline();*/
                localPlayerController.StartGame(null);
                print("All player ready, sending event to notify all players.");
                if (PhotonNetwork.IsMasterClient) OnAllPlayersReadyEvent?.Invoke();
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

        // Action when received player is ready
        void PlayerReadyEvent(EventData obj)
        {
            print("Setting this guy to ready " + obj.CustomData);

            for (int i = 0; i < playerList.Count; i++)
            {
                if (!playerList[i].GetPlayerReady())
                {
                    if (playerList[i].GetInfo.PhotonInfo.PView.ViewID == (int)obj.CustomData)
                    {
                        playerList[i].SetPlayerReady(true);
                        NetworkEventManager.Instance.RaiseEvent(ByteEvents.PLAYER_SPAWNED_CONFIRMED, (int)obj.CustomData, SendOptions.SendReliable);
                        return;
                    }
                }
            }
        }

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
                localPlayerController = controller;
                controller.HandlePhotonView(true);
                controller.SetPlayerReady(true);
            }
            // Host finish assignming ownerships and cameras
            
            AddPlayerEvent?.Invoke();
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
            GameObject player = (GameObject)Instantiate(prefab, neManager.LocalSpawn.position, neManager.LocalSpawn.rotation);
            PlayerController controller = player.GetComponent<PlayerController>();
            controller.HandlePhotonView(true);
            playerList.Add(controller);
            controller.InjectDependencies(this, photonPlayer);

            localPlayerController = controller;
            
            //create dummy players
            for(int i = 0; i < DummyPlayerCount; i++)
            {
                Transform spawnTrans = SpawnManager.instance.GetSpawnPoint();
                GameObject dummyPlayer = (GameObject)Instantiate(prefab, spawnTrans.position, spawnTrans.rotation);
                PlayerController dummyControllers = dummyPlayer.GetComponent<PlayerController>();
                dummyControllers.HandlePhotonView(false);
                playerList.Add(dummyControllers);
                dummyControllers.InjectDependencies(this, photonPlayer);
                
                if(!DummyMirrorsMovement)
                {
                    //dummyControllers.enabled = false;
                    dummyControllers.SetDummyState(true);
                }
            }
            
            controller.TrackNamesOffline();
        }

        public void InstantiatePViewList()
        {
            print("instantiate PView Called");
            var playerControllers = FindObjectsOfType<PlayerController>();
            GameManager.Instance.pViewList = new List<PhotonView>();

            if(playerControllers == null)
            {
                print("No controllers found");
            }

            for (int i=0; i < playerControllers.Length; i++)
            {
              //  print(playControllers[i].GetInfo.PhotonInfo.PView + "found");
                GameManager.Instance.pViewList.Add(playerControllers[i].GetInfo.PhotonInfo.PView);
            }  
        }

        #endregion

        public void SetDummySettings(int dummyCount, bool dummyMirrorsMovement)
        {
            DummyPlayerCount = dummyCount;
            DummyMirrorsMovement = dummyMirrorsMovement;
        }
        private string GetPrefabPath() => Path.Combine(PrefabFolder, PrefabName);
        private object[] DefaultObjectArray() => new object[] { _pView.ViewID };
        public PlayerController GetController(Photon.Realtime.Player player)
        {
            foreach (PlayerController controller in playerList) if (controller.AttachedPlayer == player) return controller;
            return null;
        }
        public PlayerController GetController(GameObject playerObject) => playerObject.GetComponent<PlayerController>();

        public bool IsOnNetwork => !NetworkEventManager.Instance.isOfflineMode;

        public PhotonView managerPView => _pView;

        public List<PlayerController> playerControllers => playerList;
    }
}