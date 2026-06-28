using UnityEngine;
using UnityEngine.InputSystem;

namespace SBAR.Interaction
{
    /// <summary>
    /// VR-tegenhanger van InteractionRaycaster. Schiet een ray vanaf de controller
    /// (transform.forward) en roept IInteractable.OnInteract aan bij triggerdruk.
    /// Zelfstandig: bindt de trigger zelf via Input System, geen inspector-refs nodig.
    /// Plaats op de Right/Left Controller van de XR Origin.
    /// </summary>
    public class VRInteractor : MonoBehaviour
    {
        [Header("Ray")]
        [SerializeField] private float maxDistance = 5f;
        [SerializeField] private LayerMask interactableMask = ~0;

        [Header("Input")]
        [Tooltip("Input System binding voor de actieknop (trigger).")]
        [SerializeField] private string triggerBinding = "<XRController>{RightHand}/triggerPressed";

        private IInteractable _current;
        private InputAction _activate;

        public bool InputEnabled { get; set; } = true;

        private void OnEnable()
        {
            _activate = new InputAction("VRInteract", InputActionType.Button, triggerBinding);
            _activate.Enable();
        }

        private void OnDisable()
        {
            _activate?.Disable();
            _activate?.Dispose();
            _activate = null;
            ClearHover();
        }

        private void Update()
        {
            if (!InputEnabled)
            {
                ClearHover();
                return;
            }

            UpdateHover();

            if (_current != null && _activate != null && _activate.WasPressedThisFrame())
                _current.OnInteract();
        }

        private void UpdateHover()
        {
            IInteractable hit = null;

            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit info,
                    maxDistance, interactableMask, QueryTriggerInteraction.Collide))
            {
                hit = info.collider.GetComponentInParent<IInteractable>();
            }

            if (hit != _current)
            {
                _current?.OnHoverExit();
                _current = hit;
                _current?.OnHoverEnter();
            }
        }

        private void ClearHover()
        {
            if (_current != null)
            {
                _current.OnHoverExit();
                _current = null;
            }
        }
    }
}
