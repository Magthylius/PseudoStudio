using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Locomotion;
using Hadal.Inputs;

namespace Hadal.Player.Aesthetics
{
    public class SubmarineFinHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField, Min(0f)] float tweenSpeed = 10f;

        [Header("References")]
        [SerializeField] SubmarineFinBehaviour leftFin;
        [SerializeField] SubmarineFinBehaviour rightFin;
        [SerializeField] Mover playerMover;
        [SerializeField] Rotator playerRotator;

        IMovementInput moveInput;
        IRotationInput rotateInput;

        int sl_MovementVec;

        void Start()
        {
            moveInput = playerMover.Input;
            rotateInput = playerRotator.Input;

            leftFin.SetTweenSpeed(tweenSpeed);
            rightFin.SetTweenSpeed(tweenSpeed);

            //sl_MovementVec = DebugManager.Instance.CreateScreenLogger();
        }

        void Update()
        {
            Vector3 movementVec = new Vector3(moveInput.HorizontalAxis, moveInput.HoverAxis, moveInput.VerticalAxis);
            UpdateFins(movementVec.normalized);

            //DebugManager.Instance.SLog(sl_MovementVec, "Movement", movementVec.normalized);
        }

        void UpdateFins(Vector3 movement)
        {
            leftFin.UpdateMovement(movement);
            rightFin.UpdateMovement(movement);
        }
    }
}
