using UnityEngine;

namespace SBAR.Interaction
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Lopen")]
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Rondkijken")]
        [SerializeField] private float lookSensitivity = 2.2f;
        [SerializeField] private float maxPitch = 85f;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private int lookMouseButton = 1;

        private CharacterController _controller;
        private float _pitch;
        private float _verticalVelocity;

        public bool MoveEnabled { get; set; } = true;
        public bool LookEnabled { get; set; } = true;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (cameraPivot == null && Camera.main != null)
                cameraPivot = Camera.main.transform;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            HandleLook();
            HandleMove();
        }

        public void HandleLook()
        {
            if (!LookEnabled || !Input.GetMouseButton(lookMouseButton)) return;

            float yaw = Input.GetAxis("Mouse X") * lookSensitivity;
            float pitchDelta = Input.GetAxis("Mouse Y") * lookSensitivity;

            transform.Rotate(Vector3.up, yaw);
            _pitch = Mathf.Clamp(_pitch - pitchDelta, -maxPitch, maxPitch);
            if (cameraPivot != null)
                cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        public void HandleMove()
        {
            Vector3 input = Vector3.zero;
            if (MoveEnabled)
            {
                float x = Input.GetAxisRaw("Horizontal");
                float z = Input.GetAxisRaw("Vertical");
                input = (transform.right * x + transform.forward * z).normalized;
            }

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -1f;
            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = input * moveSpeed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }
    }
}
