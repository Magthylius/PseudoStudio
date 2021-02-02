using Photon.Pun;
using UnityEngine;

//Created by Jet
namespace Hadal.Controls
{
    public class PlayerLamp : MonoBehaviour
    {
        [Header("Light")]
        [SerializeField] private Light playerLight;
        [SerializeField] private Color lightColour;
        [SerializeField] private float switchSpeed;
        [SerializeField] private float switchSnap;

        [Header("Settings")]
        [SerializeField] private float maxRange;
        [SerializeField] private float maxAngle;
        [SerializeField] private float rangeIncrementSpeed;
        [SerializeField] private float angleIncrementSpeed;

        private ILightInput _input;
        private PhotonView _pView;
        private float _defaultRange;
        private float _defaultAngle;
        private float _defaultIntensity;
        private Color _defaultColour;
        private bool _isOn;

        private void Awake()
        {
            _input = new StandardLightControlInput();
            _defaultRange = playerLight.range;
            _defaultAngle = playerLight.spotAngle;
            _defaultIntensity = playerLight.intensity;
            _defaultColour = playerLight.color;
            _isOn = false;
            switchSnap = Mathf.Abs(switchSnap);
            playerLight.color = lightColour;
            playerLight.intensity = 0.0f;
        }

        public void Inject(PhotonView pView) => _pView = pView;

        public void DoUpdate(in float deltaTime)
        {
            SetLightEnabled(_input.SwitchAxis, deltaTime);
            //UpdateRangeAndAngle();
        }

        public void SetLightEnabled(in bool statement, in float deltaTime)
        {
            float destination;
            if (statement) destination = _defaultIntensity;
            else destination = float.Epsilon;

            if(_isOn != statement)
            {
                bool state = statement;
                PhotonNetwork.RemoveBufferedRPCs(_pView.ViewID, nameof(RPC_SetLightEnabled));
                _pView.RPC(nameof(RPC_SetLightEnabled), RpcTarget.OthersBuffered, state);
            }
            _isOn = statement;
            playerLight.intensity = Mathf.Lerp(playerLight.intensity, destination, switchSpeed * deltaTime);
        }

        private void UpdateRangeAndAngle()
        {
            if (!_isOn) return;
            playerLight.range = Mathf.Clamp(playerLight.range + RangeIncrement, _defaultRange, maxRange);
            playerLight.spotAngle = Mathf.Clamp(playerLight.spotAngle + AngleIncrement, _defaultAngle, maxAngle);
        }

        private float DeltaTime => Time.deltaTime;

        private float RangeIncrement => _input.RangeAxis * rangeIncrementSpeed * Time.deltaTime;
        private float AngleIncrement => _input.AngleAxis * angleIncrementSpeed;

        public bool LightsOn => _isOn;

        [PunRPC]
        private void RPC_SetLightEnabled(bool statement)
        {
            _isOn = statement;
            if(statement) playerLight.intensity = _defaultIntensity;
            else playerLight.intensity = 0.0f;
        }
    }
}