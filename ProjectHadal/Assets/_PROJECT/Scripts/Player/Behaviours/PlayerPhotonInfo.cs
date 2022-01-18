using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

namespace Hadal.Player.Behaviours
{
    [RequireComponent(typeof(PhotonView))]
    public class PlayerPhotonInfo : MonoBehaviour, IPlayerComponent
    {
        public PhotonView PView; 
        public PhotonLagSimulationGui PLagSimulGUI;
        public PhotonStatsGui PStatsGUI;
        public PhotonTransformViewClassic PTransViewClassic;
        
        public void Inject(PlayerController controller) { }
    }
}