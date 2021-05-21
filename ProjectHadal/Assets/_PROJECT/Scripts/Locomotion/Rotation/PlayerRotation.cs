using Hadal.Inputs;
using UnityEngine;

//Created by Jet, E: Jon
namespace Hadal.Locomotion
{
    public class PlayerRotation : Rotator
    {
        Quaternion currentQT;
        Quaternion targetQT;

        int sl_MP;

        public override void Initialise(Transform target)
        {
            Enable();
            this.target = target;
            Rotary.Initialise();
            Input = new StandardRotationInput();

            currentQT = target.localRotation;
            targetQT = currentQT;

            sl_MP = DebugManager.Instance.CreateScreenLogger();
        }

        public override void DoUpdate(in float deltaTime)
        {
            if (!allowUpdate) return;

        }

        public override void DoFixedUpdate(in float fixedDeltaTime)
        {
            if (!allowUpdate) return;

            RotateByQT();
        }

        public override void DoLateUpdate(in float deltaTime)
        {
            
        }

        void RotateByQT()
        {
            float pitch = -Input.YAxis * Rotary.GetPitchSensitivity;
            float yaw = Input.XAxis  * Rotary.GetYawSensitivity;
            float roll = Input.ZAxis * Rotary.GetRollSensivity;

            targetQT *= Quaternion.Euler(pitch , yaw , roll);
            currentQT = Quaternion.Lerp(currentQT, targetQT, 5f * Time.deltaTime);

            target.localRotation = currentQT;

            //DebugManager.Instance.SLog(sl_MP, "Cos: " + Mathf.Cos(currentEA.z) + " | Sin: " + Mathf.Sin(currentEA.z));
            //DebugManager.Instance.SLog(sl_MP, "P: " + pitch + " | Y: " + yaw + " | CurZ: " + currentEA.z);
        }
    }
}