using Hadal.Inputs;
using UnityEngine;

//Created by Jet, E: Jon
namespace Hadal.Locomotion
{
    public class PlayerRotation : Rotator
    {
        Vector2 screenCenter = new Vector2();
        float mouseControlMinPull = 75f;
        float mouseControlMaxPull = 300f;
        float rotationSensitivity = 3f;

        Vector3 lookDirection;

        int sl_MP;

        public override void Initialise(Transform target)
        {
            Enable();
            this.target = target;
            Rotary.Initialise();
            Input = new StandardRotationInput();

            screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

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
            

            Vector3 direction = ((Vector2)UnityEngine.Input.mousePosition - screenCenter).normalized;
            float dist = ((Vector2)UnityEngine.Input.mousePosition - screenCenter).magnitude;
            float power = Mathf.Clamp((dist - mouseControlMinPull) / (mouseControlMaxPull), 0f, 1f);

            direction *= power;
            lookDirection = direction;
            direction.z = -Input.ZAxis;

            //! flip for rotation
            float temp = direction.x;
            direction.x = -direction.y;
            direction.y = temp;

            direction *= rotationSensitivity;


            //target.localRotation = Quaternion.Lerp(target.localRotation, Quaternion.Euler(targetRot), rotationSensitivity * Time.deltaTime);
            //Vector3 selfRot = target.localRotation.eulerAngles;
            //selfRot = Vector3.Lerp(selfRot, targetRot, 0.4f);

            //target.localRotation = Quaternion.Euler(selfRot);
            //target.localEulerAngles = selfRot;
            target.Rotate(direction);

            DebugManager.Instance.SLog(sl_MP, lookDirection);
        }

        public override void DoLateUpdate(in float deltaTime)
        {
            
        }


        public override Vector3 LookDirection => lookDirection;
    }
}