using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.UI.Animations
{
    public class AnimationResetter : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (var parameter in animator.parameters)
            {
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(parameter.name, false);
                        break;
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(parameter.name, 0f);
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(parameter.name, 0);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        animator.ResetTrigger(parameter.name);
                        break;
                }
            }
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
}
