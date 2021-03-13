using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Locomotion;
using Hadal.Inputs;

namespace Hadal.Player.Aesthetics
{
    public class SubmarineFinHandler : MonoBehaviour
    {
        [SerializeField] SubmarineFinBehaviour leftFin;
        [SerializeField] SubmarineFinBehaviour rightFin;
        [SerializeField] Mover playerMover;
        [SerializeField] Rotator playerRotator;

        IMovementInput moveInput;
        IRotationInput rotateInput;

        void Start()
        {
            moveInput = playerMover.Input;
            rotateInput = playerRotator.Input;
        }

        void Update()
        {
            Vector3 movementVec = new Vector3(moveInput.HorizontalAxis, moveInput.HoverAxis, moveInput.VerticalAxis);
            UpdateFins(movementVec);
        }

        void UpdateFins(Vector3 movement)
        {
            leftFin.UpdateMovement(movement);
            rightFin.UpdateMovement(movement);
        }
    }
}
