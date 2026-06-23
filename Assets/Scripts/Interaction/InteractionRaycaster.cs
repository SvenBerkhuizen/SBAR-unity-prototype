using UnityEngine;
using UnityEngine.EventSystems;

namespace SBAR.Interaction
{
    public class InteractionRaycaster : MonoBehaviour
    {
        [Header("Ray-instellingen")]
        [SerializeField] private Camera sourceCamera;
        [SerializeField] private float maxDistance = 4f;
        [SerializeField] private LayerMask interactableMask = ~0;

        [Header("Toetsen")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private IInteractable _current;

        public bool InputEnabled { get; set; } = true;

        private void Awake()
        {
            if (sourceCamera == null) sourceCamera = Camera.main;
        }

        private void Update()
        {
            if (!InputEnabled || sourceCamera == null)
            {
                ClearHover();
                return;
            }

            UpdateHover();
            HandleActivation();
        }

        public void UpdateHover()
        {
            Ray ray = sourceCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            IInteractable hit = null;

            if (Physics.Raycast(ray, out RaycastHit info, maxDistance, interactableMask, QueryTriggerInteraction.Collide))
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

        public void HandleActivation()
        {
            if (_current == null) return;

            bool pointerOverUI = EventSystem.current != null &&
                                 EventSystem.current.IsPointerOverGameObject();

            bool pressed = (Input.GetMouseButtonDown(0) && !pointerOverUI) ||
                           Input.GetKeyDown(interactKey);

            if (pressed) _current.OnInteract();
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
