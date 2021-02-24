using Hadal.Inputs;
using UnityEngine;

//Created by Jet, E: Jon
namespace Hadal.Locomotion
{
    public class PlayerRotation : Rotator
    {
        public override void Initialise(Transform target)
        {
            Enable();
            this.target = target;
            Rotary.Initialise();
            Input = new StandardRotationInput();
        }

        public override void DoUpdate(in float deltaTime)
        {
            if (!allowUpdate) return;
            //Rotary.DoSmoothRotation(Input, deltaTime, target);
            //Rotary.DoRotationWithLerp(Input, deltaTime, target);
            Rotary.DoLocalRotation(Input, deltaTime, target);
        }
    }
}