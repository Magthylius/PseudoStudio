using Hadal.Inputs;
using UnityEngine;

//Created by Jet
namespace Hadal.Locomotion
{
    public class PlayerRotation : Rotator
    {
        public override void Initialise(Transform target)
        {
            this.target = target;
            Rotary.Initialise();
            Input = new StandardRotationInput();
        }

        public override void DoUpdate(in float deltaTime)
        {
            Rotary.DoSmoothRotation(Input, deltaTime, target);
            //Rotary.DoRotationWithLerp(Input, deltaTime, target);
        }
    }
}