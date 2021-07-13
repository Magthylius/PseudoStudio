using Hadal.Inputs;
using UnityEngine;

//Created by Jet, E: Jon
namespace Hadal.Locomotion
{
    public class PlayerRotationF : Rotator
    {
        [SerializeField] Rigidbody rb;
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

            rb.angularDrag = 1;
            /*sl_MP = DebugManager.Instance.CreateScreenLogger();*/
        }

        public override void DoUpdate(in float deltaTime)
        {
            if (!allowUpdate) return;

        }

        public override void DoFixedUpdate(in float fixedDeltaTime)
        {
            //if (!allowUpdate) return;
            RototateByForce();
            CalculateRotationSpeed();
        }

        public override void DoLateUpdate(in float deltaTime)
        {
            
        }

        void RototateByForce()
        {
            Vector3 input = Vector3.zero;

            if (allowUpdate)
                input = Input.AllInputClamped(-maxInputAxisClamp, maxInputAxisClamp);

            float pitch = -input.y * Rotary.GetPitchSensitivity;
            float yaw = input.x * Rotary.GetYawSensitivity;
            float roll = input.z * Rotary.GetRollSensivity;

            float yawInfluence = 0f;
            if (input.x != 0)
                yawInfluence = - input.x / 2 ;

            Vector3 torqueDirection = new Vector3(pitch, yaw, roll);
            Vector3 torqueForce = Vector3.Scale(torqueDirection, rb.inertiaTensor);
            rb.AddRelativeTorque(torqueForce * 0.89f, ForceMode.Force);
            /*rb.AddRelativeTorque(torqueDirection, ForceMode.Acceleration);*/
            /*print(Rotary.GetPitchSensitivity);*/
            /*DebugManager.Instance.SLog(sl_MP, pitch + "|" + yaw + "|" + roll);*/
        }

        void CalculateRotationSpeed()
        {
            testLastQT = testCurrentQT;
            testCurrentQT = target.localRotation;
            float angle = Quaternion.Angle(testLastQT, testCurrentQT);
        }
    }
}