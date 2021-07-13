using System;
using System.Collections;
using UnityEngine;

namespace Tenshi.UnitySoku
{
    public static class TenshiCamera
    {
        private const float MaxAngle = 10f;
        private static bool _isShaking = false;
        private static IEnumerator _currentCameraShakeRoutine;

        public static void ShakeCamera(this MonoBehaviour instance, Camera camera, CameraShakeProperties properties, bool preventOverride = false)
        {
            if (_currentCameraShakeRoutine != null && !preventOverride) instance.StopCoroutine(_currentCameraShakeRoutine);
            if (!preventOverride && _isShaking) _isShaking = false;
            _currentCameraShakeRoutine = Shake(camera, properties);
            instance.StartCoroutine(_currentCameraShakeRoutine);
        }

        public static void ShakeCamera(Camera camera, CameraShakeProperties properties, MonoBehaviour instance, bool preventOverride = false)
        {
            if (_currentCameraShakeRoutine != null && !preventOverride) instance.StopCoroutine(_currentCameraShakeRoutine);
            if (!preventOverride && _isShaking) _isShaking = false;
            _currentCameraShakeRoutine = Shake(camera, properties);
            instance.StartCoroutine(_currentCameraShakeRoutine);
        }

        public static void BillboardToCamera(this Transform objectTransform, Camera camera) => objectTransform.forward = camera.transform.forward;
        public static void BillboardToCamera(this Camera camera, Transform objectTransform) => objectTransform.forward = camera.transform.forward;

        private static IEnumerator Shake(Camera camera, CameraShakeProperties properties)
        {
            if (!_isShaking)
            {
                _isShaking = true;

                float completionPercent = 0;
                float movePercent = 0;

                float angleRadians = properties.Angle * Mathf.Deg2Rad - Mathf.PI;
                Vector3 cPosition = camera.transform.localPosition;
                Vector3 previousWaypoint = Vector3.zero;
                Vector3 currentWaypoint = Vector3.zero;
                float moveDistance = 0;
                float speed = 0;

                Quaternion cRotation = camera.transform.localRotation;
                Quaternion targetRotation = Quaternion.identity;
                Quaternion previousRotation = Quaternion.identity;

                do
                {
                    if (movePercent >= 1 || completionPercent == 0)
                    {
                        float dampingFactor = DampCurve(completionPercent, properties.DampingPercent);

                        float noiseAngle = (UnityEngine.Random.value - 0.5f) * Mathf.PI;
                        angleRadians += Mathf.PI + noiseAngle * properties.NoisePercent;

                        currentWaypoint = cRotation * new Vector3(cPosition.x + Mathf.Cos(angleRadians), cPosition.y + Mathf.Sin(angleRadians), cPosition.z) * properties.Strength * dampingFactor;
                        previousWaypoint = cRotation * camera.transform.localPosition;
                        moveDistance = Vector3.Distance(currentWaypoint, previousWaypoint);

                        targetRotation = Quaternion.Euler(new Vector3(currentWaypoint.y, currentWaypoint.x).normalized * properties.RotationPercent * dampingFactor * MaxAngle);
                        previousRotation = camera.transform.localRotation;

                        speed = Mathf.Lerp(properties.MinSpeed, properties.MaxSpeed, dampingFactor);

                        movePercent = 0;
                    }

                    completionPercent += Time.deltaTime / properties.Duration;
                    movePercent += Time.deltaTime / moveDistance * speed;

                    camera.transform.localPosition = Vector3.Lerp(previousWaypoint, currentWaypoint, movePercent);
                    camera.transform.localPosition = new Vector3(camera.transform.localPosition.x, camera.transform.localPosition.y, properties.MaintainZ);
                    camera.transform.localRotation = Quaternion.Slerp(previousRotation, targetRotation, movePercent);

                    yield return null;
                } while (moveDistance > 0);

                _isShaking = false;
            }
        }

        private static float DampCurve(float x, float dampingPercent)
        {
            x = Mathf.Clamp01(x);
            float a = Mathf.Lerp(2, 0.25f, dampingPercent);
            float b = 1 - Mathf.Pow(x, a);
            return b * b * b;
        }
    }

    [Serializable]
    public class CameraShakeProperties
    {
        public float Angle;
        public float Strength;
        public float MaxSpeed;
        public float MinSpeed;
        public float Duration;
        [Range(0f, 1f)] public float NoisePercent;
        [Range(0f, 1f)] public float DampingPercent;
        [Range(0f, 1f)] public float RotationPercent;
        public float MaintainZ;

        public CameraShakeProperties(float angle, float strength, float maxSpeed, float minSpeed, float duration, float noisePercent, float dampingPercent, float rotationPercent, float maintainZ)
        {
            Angle = angle;
            Strength = strength;
            MaxSpeed = maxSpeed;
            MinSpeed = minSpeed;
            Duration = duration;
            NoisePercent = Mathf.Clamp01(noisePercent);
            DampingPercent = Mathf.Clamp01(dampingPercent);
            RotationPercent = Mathf.Clamp01(rotationPercent);
            MaintainZ = maintainZ;
        }
    }
}