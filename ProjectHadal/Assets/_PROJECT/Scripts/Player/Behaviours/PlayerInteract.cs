using System.Linq;
using Hadal.Inputs;
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
        private IInteractInput _iInput;
        private bool _interactionEnabled;

        private void Awake()
        {
            _iInput = new StandardInteractableInput();
            Enable();
        }

        public void Inject(PlayerController controller)
        {
            _player = controller;
        }

        public void DoUpdate(in float deltaTime)
        {
            if (!AllowUpdate) return;
            if (_iInput.InteractKey)
            {
                Collider collider = GetClosestEligibleCollider();
                if (collider == null) return;

                //! Interact
                collider.GetComponentInChildren<IInteractable>()?.Interact(PlayerViewID);
            }
        }

        /// <summary>
        /// Gets the closest collider in the interactable layermask (excluding self). Buffer size can be changed on the script inspector.
        /// </summary>
        private Collider GetClosestEligibleCollider()
        {
            //! Check for interactable colliders in nearby radius
            Collider[] results = new Collider[interactInfoBufferSize];
            Collider ownCollider = _player.GetInfo.Collider;
            
            Physics.OverlapSphereNonAlloc(PlayerPosition, interactRadius, results, interactableMask, QueryTriggerInteraction.Collide);
            Collider closestEligibleCollider = results.Where(c => c != ownCollider)
                                                    .OrderBy(c => Vector_SqrDistance(c.gameObject.transform.position, PlayerPosition))
                                                    .FirstOrDefault();

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
        private float Vector_SqrDistance(Vector3 first, Vector3 second) => (second - first).sqrMagnitude;

        #endregion
    }
}
