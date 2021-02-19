using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

namespace Hadal.Player.Behaviours
{
    [RequireComponent(typeof(PhotonView))]
    public class PlayerPhotonInfo : MonoBehaviour, IPlayerComponent
    {
        public PhotonView PView { get; private set; }
        public PhotonLagSimulationGui PLagSimulGUI { get; private set; }
        public PhotonStatsGui PStatsGUI { get; private set; }
        public PhotonTransformViewClassic PTransViewClassic { get; private set; }

        private void Awake()
        {
            PView = GetComponent<PhotonView>();
            PLagSimulGUI = GetComponent<PhotonLagSimulationGui>();
            PStatsGUI = GetComponent<PhotonStatsGui>();
            PTransViewClassic = GetComponent<PhotonTransformViewClassic>();
        }

        public void Inject(PlayerController controller) { }
    }
}