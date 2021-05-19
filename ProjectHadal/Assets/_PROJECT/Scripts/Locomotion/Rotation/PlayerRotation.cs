using Hadal.Inputs;
using UnityEngine;

//Created by Jet, E: Jon
namespace Hadal.Locomotion
{
    public class PlayerRotation : Rotator
    {
        float rotationSensitivity = 1.5f;

        Vector3 lookDirection;

        Vector3 currentEA;
        Vector3 targetEA;

        public Ray lookRay;

        int sl_MP;

        public override void Initialise(Transform target)
        {
            Enable();
            this.target = target;
            Rotary.Initialise();
            Input = new StandardRotationInput();


            //lookRay = target.forward * lookObjectDistance;
            lookRay = new Ray(target.position, target.forward);
            currentEA = target.eulerAngles;
            targetEA = currentEA;

            sl_MP = DebugManager.Instance.CreateScreenLogger();
        }

        public override void DoUpdate(in float deltaTime)
        {
            if (!allowUpdate) return;
            //Rotary.DoSmoothRotation(Input, deltaTime, target);
            //Rotary.DoRotationWithLerp(Input, deltaTime, target);
            //Rotary.SetTargetRotation(Input, deltaTime, target);
           // Rotary.DoLocalRotation(Input, deltaTime, target);
        }

        public override void DoFixedUpdate(in float fixedDeltaTime)
        {
            if (!allowUpdate) return;
            //Rotary.DoLocalRotation(Input, 1, target);
            //Rotary.DoLocalRotationFixedUpdate(Input, target);
            //Rotary.SetTargetRotation(Input, fixedDeltaTime, target);

            //RotateByMouse();
            //RotateByLookObject();
            RotateByEA();
        }

        public override void DoLateUpdate(in float deltaTime)
        {
            
        }

        void RotateByEA()
        {
            targetEA += new Vector3(-Input.YAxis, Input.XAxis, 0f) * rotationSensitivity;
            currentEA = Vector3.Lerp(currentEA, targetEA, 5f * Time.deltaTime);

            target.localEulerAngles = currentEA;

            DebugManager.Instance.SLog(sl_MP, currentEA);
        }

        public override Vector3 LookDirection => targetEA;
    }
}