using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Player
{
    public class DebugCameraLogger : MonoBehaviour
    {
   
        private PlayerManager _manager;
        public GameObject[] players;
        public List<Camera> playerCameras;
        public bool[] cameraTrue;

        int SL_Debug;

        public void InjectDependencies(PlayerManager playerManager)
        {
            _manager = playerManager;
        }

        void Start()
        {
            SL_Debug = DebugManager.Instance.CreateScreenLogger();
            players = GameObject.FindGameObjectsWithTag("Player");
            cameraTrue = new bool[4];

            /* for (int i = 0; i < _manager.playerControllers.Count; i++)
             {
                 playerCameras.Add(_manager.playerControllers[i].GetInfo.CameraController.GetCamera);
             }*/
            for (int i = 0; i < players.Length; i++)
            {
                playerCameras.Add(players[i].GetComponentInChildren<PlayerController>().GetInfo.CameraController.GetCamera);
            }
        }

        // Update is called once per frame
        void Update()
        {
            string cameras = "Cameras : " + "\n";

            players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                // playerCameras.Add(players[i].GetComponentInChildren<Camera>());
                cameraTrue[i] = players[i].GetComponentInChildren<PlayerController>().GetInfo.CameraController.GetCamera.gameObject.activeSelf;
            }

            for (int i = 0; i < cameraTrue.Length; i++)
            {
               // cameras += playerCameras[i].gameObject.activeSelf.ToString() + "\n";
                cameras += cameraTrue[i] + "\n";
            }

            DebugManager.Instance.SLog(SL_Debug, cameras);
        }
    }
}
