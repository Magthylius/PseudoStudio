using System.Collections.Generic;
using System.Linq;
using Hadal.Inputs;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Hadal.Player
{
    /// <summary>
    /// Manages what to do by listening to the <see cref="IInteractInput"/> of the current player.
    /// </summary>
    public class PlayerInteract : MonoBehaviour, IPlayerComponent, IPlayerEnabler
    {
        [Header("Settings")]
        [SerializeField, Min(0f)] private float interactRadius;
        [SerializeField] private LayerMask interactableMask;
        [SerializeField, Range(1, 4)] private int interactInfoBufferSize;
        private PlayerController _player;
        private IInteractInput _reviveInput;
        private bool _interactionEnabled;

        private void Awake()
        {
            _reviveInput = new ReviveInteractionInput();
            Enable();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }

        public void Inject(PlayerController controller)
        {
            _player = controller;
        }

        public void DoUpdate(in float deltaTime)
        {
            if (!AllowUpdate) return;

            //! Check for revive key
            if (_reviveInput.InteractKey)
            {
                Collider collider = GetClosestEligibleCollider();
                if (collider == null) return;

                //! Interact
                collider.GetComponentInChildren<IInteractable>()?.Interact(PlayerViewID);
            }

            //! check for other interaction keys here (e.g. torpedo pickup)
        }

        /// <summary>
        /// Gets the closest collider in the interactable layermask (excluding self). Buffer size can be changed on the script inspector.
        /// </summary>
        private Collider GetClosestEligibleCollider()
        {
            //! Check for interactable colliders in nearby radius
            Collider[] results = new Collider[interactInfoBufferSize];
            Collider ownCollider = _player.GetInfo.Collider;
            
            Physics.OverlapSphereNonAlloc(PlayerPosition, interactRadius, results, interactableMask.value, QueryTriggerInteraction.Collide);
            Collider closestEligibleCollider = null;

            List<Collider> colliders = new List<Collider>(results);
            
            //! Remove own collider (do not evaluate self) & all nulls
            colliders.Remove(ownCollider);
            colliders.RemoveAll(c => c == null);
            
            //! Take closest collider
            float closestDistance = float.MaxValue;
            Vector3 selfPos = PlayerPosition;
            int i = -1;
            while (++i < colliders.Count)
            {
                float distance = (colliders[i].transform.position - selfPos).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEligibleCollider = colliders[i];
                }
            }

            return closestEligibleCollider;
        }

        #region Enabler Interface

        public void Disable()
        {
            _interactionEnabled = false;
        }
        public void Enable()
        {
            _interactionEnabled = true;
        }
        public void ToggleEnablility()
        {
            if (_interactionEnabled)
                Disable();
            else
                Enable();
        }
        public bool AllowUpdate => _interactionEnabled;

        #endregion

        #region Shorthands

        private Vector3 PlayerPosition => _player.GetTarget.position;
        private int PlayerViewID => _player.GetInfo.PhotonInfo.PView.ViewID;

        #endregion
    }
}
