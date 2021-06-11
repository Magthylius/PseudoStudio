using Tenshi;
using Tenshi.UnitySoku;
using Photon.Pun;
using UnityEngine;

//Created by Jet
namespace Hadal.Player.Behaviours
{
    public class PlayerCameraController : CameraController, IPlayerComponent
    {
        #region Variable Definitions

        [Header("FOV")]
        [SerializeField] private float additionalFOVOnBoost;
        [SerializeField] private float fovBoostTransitionSpeed;
        [SerializeField] private float fovRecoverTransitionSpeed;

        [Header("Special Effects")]
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private CameraShakeProperties shakeProperties;

        private float _originalCameraFOV;
        private bool _isDisabled = false;
        private PhotonView _pView;
        private PlayerController _controller;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _originalCameraFOV = selfCamera.fieldOfView;
        }

        #endregion

        #region FOV Lerp Methods

        public void CameraTransition(in float deltaTime, in bool isBoosted)
        {
            return;
            // if (_isDisabled || true) return;
            // if (isBoosted) LerpBoostedFOV(deltaTime);
            // else LerpOriginalFOV(deltaTime);
        }
        private void LerpBoostedFOV(in float deltaTime)
        {
            float fov = selfCamera.fieldOfView;
            fov.LerpAngle(BoostedFOV, fovBoostTransitionSpeed, deltaTime);
            selfCamera.fieldOfView = fov;
        }
        private void LerpOriginalFOV(in float deltaTime)
        {
            float fov = selfCamera.fieldOfView;
            fov.LerpAngle(_originalCameraFOV, fovRecoverTransitionSpeed, deltaTime);
            selfCamera.fieldOfView = fov;
        }

        #endregion

        #region Camera Shake

        public void ShakeCameraDefault()
        {
            if (_isDisabled || !enableCameraShake) return;
            this.ShakeCamera(selfCamera, shakeProperties, true);
        }
        public void ShakeCamera(float magnitude)
        {
            if (_isDisabled || !enableCameraShake) return;
            var sProp = ShakePropertiesWithSpeed(magnitude);
            this.ShakeCamera(selfCamera, sProp, true);
        }
        private CameraShakeProperties ShakePropertiesWithSpeed(float speed)
        {
            var newShakeProperties = new CameraShakeProperties(shakeProperties.Angle, shakeProperties.Strength + speed * 20,
                shakeProperties.MaxSpeed + speed * 500, shakeProperties.MinSpeed, shakeProperties.Duration + speed * 50,
                shakeProperties.NoisePercent + speed * 20, shakeProperties.DampingPercent - speed * 10, shakeProperties.RotationPercent);
            return newShakeProperties;
        }

        #endregion

        #region Photon

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void Activate()
        {
            _isDisabled = false;
            selfCamera.enabled = true;
            selfCamera.gameObject.SetActive(true);

            var listener = selfCamera.GetComponent<AudioListener>();
            if (listener == null) selfCamera.gameObject.AddComponent<AudioListener>();
        }

        public void Deactivate()
        {
            _isDisabled = true;
            selfCamera.enabled = false;
            selfCamera.gameObject.SetActive(false);
            
            var listener = selfCamera.GetComponent<AudioListener>();
            if (listener != null) Destroy(listener);
        }

        public void Inject(PlayerController controller)
        {
            _controller = controller;
            var info = _controller.GetInfo;
            _pView = info.PhotonInfo.PView;
        }

        #endregion

        #region Shorthands

        private float BoostedFOV => _originalCameraFOV + additionalFOVOnBoost;
        public Camera GetCamera => selfCamera;

        #endregion
    }
}