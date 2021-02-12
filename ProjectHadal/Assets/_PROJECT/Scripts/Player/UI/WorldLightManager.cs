using Photon.Pun;
using UnityEngine;

namespace Hadal.Player
{
    public class WorldLightManager : MonoBehaviour
    {
        [SerializeField] private GameObject betterLights;
        [SerializeField] private GameObject normalLights;
        private PhotonView _pView;

        private void Awake() => _pView = GetComponent<PhotonView>();
        private void Update()
        {
            if (betterLights == null || normalLights == null || !PhotonNetwork.IsMasterClient) return;
            if(Input.GetKeyDown(KeyCode.L))
            {
                PhotonNetwork.RemoveBufferedRPCs(_pView.ViewID, nameof(RPC_ToggleWorldLights));
                _pView.RPC(nameof(RPC_ToggleWorldLights), RpcTarget.AllBuffered);
            }
        }

        [PunRPC]
        private void RPC_ToggleWorldLights()
        {
            betterLights.SetActive(!betterLights.activeInHierarchy);
            normalLights.SetActive(!normalLights.activeInHierarchy);
        }
    }
}