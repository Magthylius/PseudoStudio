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
        [SerializeField] Rigidbody referenceRigidbody;
        [SerializeField] PlayerMovementF playerMovement;
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

            sl_MovementVec = DebugManager.Instance.CreateScreenLogger();
        }

        void FixedUpdate()
        {
            Vector3 movement = referenceRigidbody.transform.InverseTransformDirection(referenceRigidbody.velocity);
            //leftFin.transform.parent.GetComponent<Rigidbody>().AddForce(-movement);
            //rightFin.transform.parent.GetComponent<Rigidbody>().AddForce(-movement);
            //return;
            //Vector3 movementVec = new Vector3(moveInput.HorizontalAxis, moveInput.HoverAxis, moveInput.VerticalAxis);
            
            float magnitude = (movement.magnitude / playerMovement.Speed.Max) * 2f;
            UpdateFins(movement, playerMovement.Speed.Normalised);
            DebugManager.Instance.SLog(sl_MovementVec, "Movement", magnitude);
        }

        void UpdateFins(Vector3 movement, float magnitude)
        {
            leftFin.UpdateMovement(movement, magnitude);
            rightFin.UpdateMovement(movement, magnitude);
        }
    }
}
