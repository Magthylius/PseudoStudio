using Hadal.Utility;
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
        [SerializeField] private CameraUtility.CameraShakeProperties shakeProperties;

        private float _originalCameraFOV;
        private float _sqrSpeed = 0;
        private bool _isDisabled = false;
        private PhotonView _pView;

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
            if (_isDisabled) return;
            if (isBoosted) LerpBoostedFOV(deltaTime);
            else LerpOriginalFOV(deltaTime);
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

        public void SetSqrSpeed(float speed) => _sqrSpeed = speed;
        public void ShakeCameraDefault()
        {
            if (_isDisabled) return;
            this.ShakeCamera(selfCamera, shakeProperties, true);
        }
        public void ShakeCamera()
        {
            if (_isDisabled) return;
            var sProp = ShakePropertiesWithSpeed(_sqrSpeed);
            this.ShakeCamera(selfCamera, sProp, true);
        }
        private CameraUtility.CameraShakeProperties ShakePropertiesWithSpeed(float speed)
        {
            var newShakeProperties = new CameraUtility.CameraShakeProperties(shakeProperties.Angle, shakeProperties.Strength + speed * 20,
                shakeProperties.MaxSpeed + speed * 500, shakeProperties.MinSpeed, shakeProperties.Duration + speed * 50,
                shakeProperties.NoisePercent + speed * 20, shakeProperties.DampingPercent - speed * 10, shakeProperties.RotationPercent);

            return newShakeProperties;
        }

        #endregion

        #region Photon

        public void Deactivate()
        {
            if (_pView.IsMine) return;
            _isDisabled = true;
            selfCamera.enabled = false;
            selfCamera.GetComponent<AudioListener>().enabled = false;
        }

        public void Inject(PlayerController controller)
        {
            var info = controller.GetInfo;
            _pView = info.PhotonView;
        }

        #endregion

        #region Shorthands

        private float BoostedFOV => _originalCameraFOV + additionalFOVOnBoost;
        public Camera GetCamera => selfCamera;

        #endregion
    }
}