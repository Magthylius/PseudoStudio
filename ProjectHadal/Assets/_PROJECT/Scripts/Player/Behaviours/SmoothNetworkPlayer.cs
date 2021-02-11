using Photon.Pun;
using UnityEngine;

namespace Hadal.Player.Behaviours
{
    public class SmoothNetworkPlayer : MonoBehaviourPun, IPunObservable
    {
        [SerializeField] private PlayerController controller;
        [SerializeField] private float smoothingDelay;
		private Vector3 _lastPos;
        private Quaternion _lastRot;
        private bool _received = false;
        private float _currentTime;
        private double _currentPacket;
        private double _lastPacket;
        private Vector3 _posLastPacket;
        private Quaternion _rotLastPacket;
        private PhotonView _pView;

        private void Awake() => _pView = GetComponent<PhotonView>();
        private void Start()
        {
            if(!_pView.ObservedComponents.Contains(this))
            {
                _pView.ObservedComponents.Add(this);
            }
			
			PhotonNetwork.SendRate = 20;
			PhotonNetwork.SerializationRate = 10;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
				print("Writing");
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            if(stream.IsReading)
			{
				print("Reading");
				_lastPos = (Vector3)stream.ReceiveNext();
				_lastRot = (Quaternion)stream.ReceiveNext();
				
				_received = true;
				_currentTime = 0.0f;
				_lastPacket = _currentPacket;
				_currentPacket = info.SentServerTime;
				_posLastPacket = transform.position;
				_rotLastPacket = transform.rotation;
			}
        }

        public void DoUpdate(in float deltaTime)
        {
            if (_pView.IsMine || !_received) return;
            _currentTime += deltaTime;
            float lagDelta = (float)(_currentTime / PacketDifference);
            transform.position = Vector3.Lerp(_posLastPacket, _lastPos, lagDelta);
            transform.rotation = Quaternion.Lerp(_rotLastPacket, _lastRot, lagDelta);
            print($"position: {transform.position}, rotation: {transform.rotation}");
        }

        private double PacketDifference => _currentPacket - _lastPacket;
    }
}