using Hadal.Inputs;
using UnityEngine;

//Created by Jet, E: Jon
namespace Hadal.Locomotion
{
    public class PlayerRotation : Rotator
    {
        [SerializeField, Min(0f)] float maxInputAxisClamp = 5f;
        [SerializeField, Range(0f, 1f)] float yawInfluenceOnRollFactor = 0.3f;

        Quaternion currentQT;
        Quaternion targetQT;

        Quaternion testCurrentQT;
        Quaternion testLastQT;
        int sl_MP;

        public override void Initialise(Transform target)
        {
            Enable();
            this.target = target;
            Rotary.Initialise();
            Input = new StandardRotationInput();

            currentQT = target.localRotation;
            targetQT = currentQT;

            testCurrentQT = currentQT;
            testLastQT = currentQT;

            /*sl_MP = DebugManager.Instance.CreateScreenLogger();*/
        }

        public override void DoUpdate(in float deltaTime)
        {
            if (!allowUpdate) return;

        }

        public override void DoFixedUpdate(in float fixedDeltaTime)
        {
            //if (!allowUpdate) return;

            RotateByQT();
            CalculateRotationSpeed();
        }

        public override void DoLateUpdate(in float deltaTime)
        {
            
        }

        void RotateByQT()
        {
            Vector3 input = Vector3.zero;

            if (allowUpdate)
                input = Input.AllInputClamped(-maxInputAxisClamp, maxInputAxisClamp);


            float pitch = -input.y * Rotary.GetPitchSensitivity;
            float yaw = input.x * Rotary.GetYawSensitivity;
            float roll = input.z * Rotary.GetRollSensivity;

            float yawInfluence = 0f;
            if (yaw != 0)
                yawInfluence = -90f * yawInfluenceOnRollFactor * yaw;

            targetQT *= Quaternion.Euler(pitch, yaw, roll);
            Quaternion yawInfluencedQT = targetQT * Quaternion.Euler(0f, 0f, yawInfluence);

            currentQT = Quaternion.Lerp(currentQT, yawInfluencedQT, 5f * Time.deltaTime);
            target.localRotation = currentQT;

        }

        public void AddRotation(Vector3 normalizedDirection, float force)
        {
            targetQT *= Quaternion.Euler(normalizedDirection * force);
        }

        void CalculateRotationSpeed()
        {
            testLastQT = testCurrentQT;
            testCurrentQT = target.localRotation;
            float angle = Quaternion.Angle(testLastQT, testCurrentQT);
        }
    }
}