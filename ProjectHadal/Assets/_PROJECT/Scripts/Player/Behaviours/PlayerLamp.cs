using Hadal.Inputs;
using Photon.Pun;
using UnityEngine;

//Created by Jet
namespace Hadal.Player.Behaviours
{
    public class PlayerLamp : MonoBehaviour, IUnityServicer, IPlayerComponent
    {
        [Header("Light")]
        [SerializeField] private new Light light; 
        [SerializeField] private Color colour;
        [SerializeField] private float toggleSpeed;
        [SerializeField] private float toggleLerpSnap;

        [Header("Settings")]
        [SerializeField] private bool startLightsOn = false;
        [SerializeField] private bool enableLightToggle = true;
        [SerializeField] private bool enableStartupOverride = false;
        [SerializeField] private float startingIntensity;
        [SerializeField] private float startingRange;
        [SerializeField] private float innerSpotAngle;
        [SerializeField] private float outerSpotAngle;

        private ILightInput _input;
        private PhotonView _pView;
        private float _defaultIntensity;
        private bool _isOn;

        private void Awake()
        {
            _input = new StandardLightControlInput();
            SetCustomLightSettings();
            SetDefaultLightSettings();
        }

        public void DoUpdate(in float deltaTime)
        {
            if (!enableLightToggle) return;
            SetLightEnabled(_input.SwitchAxis, deltaTime);
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
            light.intensity = Mathf.Lerp(light.intensity, destination, toggleSpeed * deltaTime);
        }

        public float DeltaTime => Time.deltaTime;
        public float ElapsedTime => Time.time;
        public bool LightsOn => _isOn;

        [PunRPC]
        private void RPC_SetLightEnabled(bool statement)
        {
            _isOn = statement;
            if(statement) light.intensity = _defaultIntensity;
            else light.intensity = 0.0f;
        }

        private void SetDefaultLightSettings()
        {
            _defaultIntensity = light.intensity;
            _isOn = false;
            toggleLerpSnap = Mathf.Abs(toggleLerpSnap);
            light.color = colour;
            light.intensity = 0.0f;

            if (startLightsOn)
            {
                _isOn = true;
                _input.Toggle();
                light.intensity = startingIntensity;
            }
        }

        private void SetCustomLightSettings()
        {
            if (!enableStartupOverride) return;
            light.innerSpotAngle = innerSpotAngle;
            light.spotAngle = outerSpotAngle;
            light.intensity = startingIntensity;
            light.range = startingRange;
        }

        private void OnValidate()
        {
            if (!enableLightToggle) startLightsOn = true;
        }

        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _pView = info.PhotonInfo.PView;
        }
    }
}